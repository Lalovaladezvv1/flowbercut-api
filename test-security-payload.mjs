import crypto from 'node:crypto';

const BASE_URL = 'http://localhost:5237';

function encryptPayload({
  plaintext,
  sessionKey,
  sessionId,
  requestId,
  method,
  path,
}) {
  const iv = crypto.randomBytes(12);

  const timestamp = Date.now();

  const aad = Buffer.from(
    `request|${sessionId}|${requestId}|${method}|${path}|${timestamp}`,
    'utf8'
  );

  const cipher = crypto.createCipheriv(
    'aes-256-gcm',
    sessionKey,
    iv
  );

  cipher.setAAD(aad);

  const ciphertext = Buffer.concat([
    cipher.update(
      Buffer.from(plaintext, 'utf8')
    ),
    cipher.final(),
  ]);

  const tag = cipher.getAuthTag();

  return {
    version: 1,
    algorithm: 'A256GCM',
    keyId: 'session',
    sessionId,
    requestId,
    timestamp,
    iv: iv.toString('base64'),
    ciphertext: ciphertext.toString('base64'),
    tag: tag.toString('base64'),
  };
}

function decryptResponse({
  envelope,
  sessionKey,
  method,
  path,
}) {
  const iv = Buffer.from(
    envelope.iv,
    'base64'
  );

  const ciphertext = Buffer.from(
    envelope.ciphertext,
    'base64'
  );

  const tag = Buffer.from(
    envelope.tag,
    'base64'
  );

  const aad = Buffer.from(
    `response|${envelope.sessionId}|${envelope.requestId}|${method}|${path}|200|${envelope.timestamp}`,
    'utf8'
  );

  const decipher = crypto.createDecipheriv(
    'aes-256-gcm',
    sessionKey,
    iv
  );

  decipher.setAAD(aad);
  decipher.setAuthTag(tag);

  return Buffer.concat([
    decipher.update(ciphertext),
    decipher.final(),
  ]).toString('utf8');
}

// ---------------------------------------------------------
// 1. Obtener public key
// ---------------------------------------------------------

const publicKeyResponse = await fetch(
  `${BASE_URL}/api/v1/security/public-key`
);

const publicKeyData =
  await publicKeyResponse.json();

console.log('✓ Public key obtenida');

// ---------------------------------------------------------
// 2. Generar AES-256
// ---------------------------------------------------------

const sessionKey =
  crypto.randomBytes(32);

console.log(
  '✓ AES-256 generado (32 bytes)'
);

// ---------------------------------------------------------
// 3. RSA-OAEP-256
// ---------------------------------------------------------

const wrappedKey =
  crypto.publicEncrypt(
    {
      key: publicKeyData.publicKey,
      padding: crypto.constants.RSA_PKCS1_OAEP_PADDING,
      oaepHash: 'sha256',
    },
    sessionKey
  );

console.log(
  '✓ AES-256 cifrado con RSA-OAEP-256'
);

// ---------------------------------------------------------
// 4. Crear sesión
// ---------------------------------------------------------

const sessionResponse = await fetch(
  `${BASE_URL}/api/v1/security/session`,
  {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      keyId: publicKeyData.keyId,
      wrappedKey: wrappedKey.toString('base64'),
    }),
  }
);

if (!sessionResponse.ok) {
  throw new Error(
    `No se pudo crear la sesión: HTTP ${sessionResponse.status}`
  );
}

const sessionData =
  await sessionResponse.json();

console.log('✓ Sesión AES creada');

console.log(
  `  SessionId: ${sessionData.sessionId}`
);

// ---------------------------------------------------------
// 5. Crear UN SOLO RequestId
// ---------------------------------------------------------

const method = 'POST';
const path = '/api/v1/security/test';

const requestId =
  crypto.randomUUID().replaceAll('-', '');

console.log(
  `  RequestId: ${requestId}`
);

// ---------------------------------------------------------
// 6. Crear payload
// ---------------------------------------------------------

const payload = {
  message: 'Hola FLOWBERCUT',
  test: true,
  timestamp: new Date().toISOString(),
};

// ---------------------------------------------------------
// 7. Cifrar payload
// ---------------------------------------------------------

const envelope = encryptPayload({
  plaintext: JSON.stringify(payload),
  sessionKey,
  sessionId: sessionData.sessionId,
  requestId,
  method,
  path,
});

console.log(
  '✓ Payload cifrado con AES-256-GCM'
);

// ---------------------------------------------------------
// 8. PRIMER REQUEST
// ---------------------------------------------------------

console.log('\n========== PRIMER REQUEST ==========');

const firstResponse = await fetch(
  `${BASE_URL}${path}`,
  {
    method,
    headers: {
      'Content-Type': 'application/json',
      'X-Flow-Session-Id': sessionData.sessionId,
      'X-Request-Id': requestId,
    },
    body: JSON.stringify(envelope),
  }
);

const firstResponseText =
  await firstResponse.text();

console.log(
  `HTTP ${firstResponse.status}`
);

if (firstResponse.ok) {
  console.log(
    '✓ Primer request aceptado correctamente'
  );

  const firstEnvelope =
    JSON.parse(firstResponseText);

  const decryptedResponse =
    decryptResponse({
      envelope: firstEnvelope,
      sessionKey,
      method,
      path,
    });

  console.log(
    '✓ Primera respuesta descifrada:'
  );

  console.log(
    JSON.stringify(
      JSON.parse(decryptedResponse),
      null,
      2
    )
  );
} else {
  console.log(
    'Respuesta:',
    firstResponseText
  );
}

// ---------------------------------------------------------
// 9. SEGUNDO REQUEST
//    MISMO RequestId
//    MISMO envelope
// ---------------------------------------------------------

console.log('\n========== SEGUNDO REQUEST ==========');

const secondResponse = await fetch(
  `${BASE_URL}${path}`,
  {
    method,
    headers: {
      'Content-Type': 'application/json',
      'X-Flow-Session-Id': sessionData.sessionId,
      'X-Request-Id': requestId,
    },
    body: JSON.stringify(envelope),
  }
);

const secondResponseText =
  await secondResponse.text();

console.log(
  `HTTP ${secondResponse.status}`
);

console.log(
  'Respuesta:',
  secondResponseText
);

// ---------------------------------------------------------
// 10. Validar Replay Protection
// ---------------------------------------------------------

if (firstResponse.status === 200 &&
    secondResponse.status === 400) {
  console.log(
    '\n🎉 REPLAY PROTECTION FUNCIONANDO CORRECTAMENTE.'
  );
} else {
  console.log(
    '\n❌ El resultado no coincide con lo esperado.'
  );
}

