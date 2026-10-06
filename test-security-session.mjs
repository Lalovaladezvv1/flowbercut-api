import crypto from 'node:crypto';

const API_URL = 'http://localhost:5237';

const publicKeyResponse = await fetch(
  `${API_URL}/api/v1/security/public-key`
);

if (!publicKeyResponse.ok) {
  throw new Error(
    `No se pudo obtener la public key: ${publicKeyResponse.status}`
  );
}

const publicKeyData = await publicKeyResponse.json();

console.log('Public key obtenida:');
console.log(`  KeyId: ${publicKeyData.keyId}`);
console.log(`  Algorithm: ${publicKeyData.algorithm}`);

const sessionKey = crypto.randomBytes(32);

console.log('\nAES-256 generado:');
console.log(`  Bytes: ${sessionKey.length}`);

const wrappedKey = crypto.publicEncrypt(
  {
    key: publicKeyData.publicKey,
    padding: crypto.constants.RSA_PKCS1_OAEP_PADDING,
    oaepHash: 'sha256',
  },
  sessionKey
);

console.log('\nAES-256 cifrado con RSA-OAEP-256:');
console.log(`  WrappedKey bytes: ${wrappedKey.length}`);

const sessionResponse = await fetch(
  `${API_URL}/api/v1/security/session`,
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

const sessionBody = await sessionResponse.text();

console.log('\nRespuesta del API:');
console.log(`  HTTP ${sessionResponse.status}`);
console.log(sessionBody);

if (!sessionResponse.ok) {
  process.exit(1);
}

const session = JSON.parse(sessionBody);

console.log('\nHandshake RSA → AES completado correctamente.');
console.log(`  SessionId: ${session.sessionId}`);
console.log(`  Algorithm: ${session.algorithm}`);
console.log(`  ExpiresAt: ${session.expiresAt}`);
