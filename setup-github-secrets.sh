#!/bin/bash
# Script para configurar los secrets de GitHub para build firmado
# Ejecutar después de crear el keystore

set -e

echo "=== Configuración de GitHub Secrets para Build Firmado ==="
echo ""

# Verificar que el keystore existe
KEYSTORE_FILE="multiformatris-release.keystore"
if [ ! -f "$KEYSTORE_FILE" ]; then
    echo "ERROR: Keystore no encontrado: $KEYSTORE_FILE"
    echo "Ejecuta primero: keytool -genkeypair -v -keystore $KEYSTORE_FILE -alias multiformatris -keyalg RSA -keysize 2048 -validity 10000"
    exit 1
fi

echo "Keystore encontrado: $KEYSTORE_FILE"
echo ""

# Codificar keystore en base64
KEYSTORE_BASE64=$(base64 -w 0 "$KEYSTORE_FILE")

echo "=== Secrets que necesitas crear en GitHub ==="
echo ""
echo "Ve a: https://github.com/Domi197669/multiformatris/settings/secrets/actions"
echo ""
echo "1. Secret: KEYSTORE_BASE64"
echo "   Value: (ejecuta el siguiente comando y pega la salida)"
echo "   base64 -w 0 $KEYSTORE_FILE"
echo ""
echo "2. Secret: KEYSTORE_PASSWORD"
echo "   Value: multiformatris123"
echo ""
echo "3. Secret: KEY_ALIAS"
echo "   Value: multiformatris"
echo ""
echo "4. Secret: KEY_PASSWORD"
echo "   Value: multiformatris123"
echo ""
echo "=== También necesitas ==="
echo ""
echo "5. Secret: UNITY_LICENSE"
echo "   (Contenido del archivo Unity_lic.ulf)"
echo ""

# Crear archivo con los secrets
cat > github-secrets.txt << EOF
=== GitHub Secrets para Multiformatris ===

Ve a: https://github.com/Domi197669/multiformatris/settings/secrets/actions

1. KEYSTORE_BASE64
$(base64 -w 0 $KEYSTORE_FILE)

2. KEYSTORE_PASSWORD
multiformatris123

3. KEY_ALIAS
multiformatris

4. KEY_PASSWORD
multiformatris123

5. UNITY_LICENSE
(Contenido del archivo Unity_lic.ulf)
EOF

echo "Archivo creado: github-secrets.txt"
echo "Copia los valores de ahí para crear los secrets en GitHub."
