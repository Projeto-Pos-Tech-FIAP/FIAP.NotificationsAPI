#!/usr/bin/env bash
# Gera o certificado autoassinado (CA + servidor) e as senhas usadas pelo
# listener TUNNEL do Kafka (SASL_SSL), que a AWS Lambda usa pra consumir os
# tópicos via túnel ngrok. Cada pessoa que rodar o projeto gera os seus
# próprios — os arquivos ficam em kafka-certs/, que é gitignored.
#
# Uso:
#   1. Rode `ngrok tcp 9094` e copie o host do "Forwarding" (SEM porta e
#      SEM "tcp://"), ex: 0.tcp.sa.ngrok.io
#   2. ./generate-kafka-tunnel-certs.sh 0.tcp.sa.ngrok.io
#   3. Cole os valores impressos no seu arquivo .env (veja .env.example)

set -euo pipefail

if [ -z "${1:-}" ]; then
  echo "Uso: $0 <host-do-tunel-ngrok-sem-porta>"
  echo "Exemplo: $0 0.tcp.sa.ngrok.io"
  exit 1
fi

TUNNEL_HOST="$1"
CERTS_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/kafka-certs"
mkdir -p "$CERTS_DIR"
cd "$CERTS_DIR"

KEYSTORE_PASS=$(openssl rand -hex 16)
SASL_PASS=$(openssl rand -hex 24)

openssl req -x509 -newkey rsa:2048 -days 365 -nodes \
  -keyout ca-key.pem -out ca-cert.pem \
  -subj "/CN=FIAP-Kafka-Tunnel-CA" \
  -addext "basicConstraints=critical,CA:true" \
  -addext "keyUsage=critical,keyCertSign,cRLSign" >/dev/null 2>&1

openssl req -newkey rsa:2048 -nodes \
  -keyout server-key.pem -out server-csr.pem \
  -subj "/CN=$TUNNEL_HOST" >/dev/null 2>&1

cat > server-ext.cnf <<EOF
basicConstraints=CA:FALSE
keyUsage=digitalSignature,keyEncipherment
extendedKeyUsage=serverAuth
subjectAltName=DNS:$TUNNEL_HOST
EOF

openssl x509 -req -in server-csr.pem -CA ca-cert.pem -CAkey ca-key.pem -CAcreateserial \
  -out server-cert.pem -days 365 -extfile server-ext.cnf >/dev/null 2>&1

echo "$KEYSTORE_PASS" > keystore.pass

openssl pkcs12 -export \
  -in server-cert.pem -inkey server-key.pem -certfile ca-cert.pem \
  -out kafka.keystore.p12 -name kafka -passout pass:"$KEYSTORE_PASS" >/dev/null 2>&1

echo ""
echo "Certificados gerados em $CERTS_DIR"
echo ""
echo "Cole essas linhas no seu .env (na raiz do FIAP.NotificationsAPI e do FIAP.Orchestration):"
echo ""
echo "TUNNEL_ADVERTISED_ADDRESS=$TUNNEL_HOST:<PORTA_DO_NGROK>"
echo "KAFKA_TUNNEL_SASL_PASSWORD=$SASL_PASS"
echo "KAFKA_TUNNEL_KEYSTORE_PASSWORD=$KEYSTORE_PASS"
echo ""
echo "(a CA gerada em ca-cert.pem também precisa ser enviada pro AWS Secrets Manager"
echo " como SERVER_ROOT_CA_CERTIFICATE do Event Source Mapping — veja o README)"
