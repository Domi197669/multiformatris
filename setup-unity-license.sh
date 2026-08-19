#!/bin/bash
# Script para activar Unity License en CI/CD
# Ejecutar localmente para obtener el archivo de licencia

set -e

UNITY_VERSION="6000.3.22f1"
LICENSE_FILE="Unity_lic.ulf"

echo "=== Unity License Activation Script ==="
echo "Unity Version: $UNITY_VERSION"
echo ""

# Verificar si Unity está instalado
UNITY_PATH=""
if command -v unity &> /dev/null; then
    UNITY_PATH="unity"
elif [ -f "/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity" ]; then
    UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity"
elif [ -f "/opt/unity/Editor/Unity" ]; then
    UNITY_PATH="/opt/unity/Editor/Unity"
elif [ -f "C:\\Program Files\\Unity\\Hub\\Editor\\$UNITY_VERSION\\Editor\\Unity.exe" ]; then
    UNITY_PATH="C:\\Program Files\\Unity\\Hub\\Editor\\$UNITY_VERSION\\Editor\\Unity.exe"
fi

if [ -z "$UNITY_PATH" ]; then
    echo "ERROR: Unity no encontrado"
    echo "Por favor instala Unity $UNITY_VERSION desde https://unity.com/download"
    exit 1
fi

echo "Unity encontrado en: $UNITY_PATH"
echo ""

# Paso 1: Crear archivo de request
echo "Paso 1: Creando archivo de request..."
cat > request.alf << EOF
<?xml version="1.0" encoding="UTF-8"?>
<root>
    <Username>TuNombreDeUsuario</Username>
    <Email>TuEmail@ejemplo.com</Email>
    <Serial>XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX</Serial>
    <StartDate>$(date +%Y-%m-%dT%H:%M:%S)</StartDate>
    <InitialActivationPath>$(pwd)/$LICENSE_FILE</InitialActivationPath>
    <ClientProvidedVersion>$UNITY_VERSION</ClientProvidedVersion>
    <ClientProvidedArchitecture>x86_64</ClientProvidedArchitecture>
    <ClientProvidedPlatform>Linux</ClientProvidedPlatform>
</root>
EOF

echo "Archivo request.alf creado"
echo ""

# Paso 2: Activar licencia
echo "Paso 2: Activando licencia..."
echo "Esto abrirá el navegador para completar la activación"
echo ""
echo "IMPORTANTE: Usa tu cuenta de Unity Personal (gratuita)"
echo ""

$UNITY_PATH -batchmode -manualLicenseFile request.alf -logFile - || true

echo ""
echo "=== Instrucciones ==="
echo "1. Se abrió tu navegador con la página de activación"
echo "2. Copia el contenido de la página"
echo "3. Pégalo en un archivo llamado '$LICENSE_FILE'"
echo "4. Ejecuta: export UNITY_LICENSE=\$(cat $LICENSE_FILE)"
echo "5. O agrega el contenido como secret 'UNITY_LICENSE' en GitHub"
echo ""
echo "=== Para GitHub Secrets ==="
echo "1. Ve a https://github.com/Domi197669/multiformatris/settings/secrets/actions"
echo "2. Click 'New repository secret'"
echo "3. Name: UNITY_LICENSE"
echo "4. Value: (contenido del archivo $LICENSE_FILE)"
echo "5. Click 'Add secret'"
