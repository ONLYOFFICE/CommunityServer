#!/bin/bash

# Creates a per-installation ASP.NET machineKey (ViewState MAC) for WebStudio.
# The key is generated once and kept in the config directory, so package
# upgrades that ship the AutoGenerate default don't reset it.

set -e

APP_ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
PRODUCT="$(basename "${APP_ROOT_DIR}")"
APP_CONFIG_DIR="/etc/${PRODUCT}/communityserver"
MACHINEKEY_FILE="${APP_CONFIG_DIR}/web.machinekey.config"

if [ ! -s "${MACHINEKEY_FILE}" ]; then
	VALIDATION_KEY=$(head -c 64 /dev/urandom | od -An -v -tx1 | tr -d ' \n')
	DECRYPTION_KEY=$(head -c 32 /dev/urandom | od -An -v -tx1 | tr -d ' \n')

	mkdir -p "${APP_CONFIG_DIR}"
	TMP_FILE=$(mktemp "${APP_CONFIG_DIR}/web.machinekey.XXXXXX")
	cat > "${TMP_FILE}" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<machineKey validationKey="${VALIDATION_KEY}" decryptionKey="${DECRYPTION_KEY}" validation="HMACSHA256" decryption="AES"/>
EOF
	mv -f "${TMP_FILE}" "${MACHINEKEY_FILE}"
fi

install -m 640 -o root -g "${PRODUCT}" "${MACHINEKEY_FILE}" "${APP_ROOT_DIR}/WebStudio/web.machinekey.config"
