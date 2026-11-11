#!/usr/bin/env sh
set -eu

project_dir=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
cd "$project_dir"

command -v dotnet >/dev/null 2>&1 || {
    printf '%s\n' 'The .NET SDK is required to create the development certificate.' >&2
    exit 1
}
command -v openssl >/dev/null 2>&1 || {
    printf '%s\n' 'OpenSSL is required to generate a local certificate password.' >&2
    exit 1
}

dotnet_major_version=$(dotnet --version | cut -d. -f1)
if [ "$dotnet_major_version" -lt 10 ]; then
    printf '%s\n' '.NET 10 SDK is required for the development certificate DNS aliases.' >&2
    exit 1
fi

mkdir -p .certs
chmod 755 .certs

env_file="$project_dir/.env"
if [ ! -f "$env_file" ]; then
    : > "$env_file"
fi
chmod 600 "$env_file"

certificate_password=$(sed -n 's/^LOCAL_TLS_CERT_PASSWORD=//p' "$env_file" | tail -n 1)
if [ -z "$certificate_password" ]; then
    if grep -q '^LOCAL_TLS_CERT_PASSWORD=' "$env_file"; then
        printf '%s\n' 'LOCAL_TLS_CERT_PASSWORD is present but empty in .env; remove that entry and rerun.' >&2
        exit 1
    fi
    certificate_password=$(openssl rand -hex 32)
    if [ -s "$env_file" ] && [ "$(tail -c 1 "$env_file" | wc -l | tr -d ' ')" -eq 0 ]; then
        printf '\n' >> "$env_file"
    fi
    printf 'LOCAL_TLS_CERT_PASSWORD=%s\n' "$certificate_password" >> "$env_file"
fi

case "$certificate_password" in
    *[!0123456789abcdefABCDEF]* )
        printf '%s\n' 'LOCAL_TLS_CERT_PASSWORD in .env must contain only hexadecimal characters.' >&2
        exit 1
        ;;
esac

dotnet dev-certs https \
    --export-path "$project_dir/.certs/localhost.crt" \
    --format PEM \
    --password "$certificate_password"

if ! dotnet dev-certs https --trust; then
    printf '%s\n' 'The certificate was exported, but this platform could not trust it automatically.' >&2
    printf '%s\n' 'Trust .certs/localhost.crt in your operating system or browser before opening HTTPS sites.' >&2
fi

if ! openssl x509 -in .certs/localhost.crt -noout -ext subjectAltName | grep -Fq 'DNS:localhost' || \
   ! openssl x509 -in .certs/localhost.crt -noout -ext subjectAltName | grep -Fq 'DNS:*.dev.localhost'; then
    printf '%s\n' 'The exported certificate must include localhost and *.dev.localhost SANs.' >&2
    exit 1
fi

chmod 644 .certs/localhost.crt .certs/localhost.key
printf '%s\n' 'Development certificate and encrypted key exported for the API and Keycloak.'
