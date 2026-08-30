# Production deployment checklist

This application is prepared for a controlled ASP.NET Core and Next.js deployment. Do not copy development configuration, uploaded files, or user secrets to source control.

## Before deployment

1. Take and verify a full SQL Server backup and a separate, versioned backup of the configured `FileStorage:RootPath` directory. The latter contains uploaded documents and generated certificates.
2. Create a dedicated SQL Server login/service identity with only the database permissions required by the application. Put its connection string in the host secret store as `ConnectionStrings__DefaultConnection`.
3. Put a randomly generated 32-byte-or-longer JWT key in the host secret store as `Jwt__Key`; also configure `Jwt__Issuer`, `Jwt__Audience`, `Cors__AllowedOrigins__0`, and `FileStorage__RootPath`.
4. Set `ASPNETCORE_ENVIRONMENT=Production`. Production startup deliberately refuses an empty/default JWT key or database connection and never runs seed/migration logic automatically.
5. Use HTTPS behind the production reverse proxy, configure the application domain in CORS, and expose only the public frontend/API routes. `/health` is an availability-only endpoint.

## Database migration procedure

1. Back up the database and verify restore in a non-production instance.
2. Publish the API artifact.
3. Apply the reviewed EF migration during the controlled maintenance window using the production configuration/service identity:

```powershell
dotnet ef database update --project backend/src/SMC.Infrastructure --startup-project backend/src/SMC.API --configuration Release
```

The `20260829090000_AddSmsEvents` migration only adds the `SmsEvents` table and two non-destructive indexes. It does not change or remove existing application data.

4. Start the API and verify `GET /health` returns HTTP 200. Verify authenticated functionality separately with a non-production account before opening public traffic.

## Backup and recovery

- Back up SQL Server on the municipality-approved schedule and retain restore-point records.
- Back up uploaded files and generated certificates together with the database backup window.
- To restore: restore the database to the selected point, restore file storage from the matching backup, set the same production secrets, run only reviewed forward migrations, then validate `/health`, authenticated downloads, and audit history.

## SMS and Bank of India QR

SMS delivery is intentionally disabled until the office supplies an approved provider/DLT setup. The application records `SmsEvents` for each workflow notification; no real SMS is sent while `Sms:Enabled` is false. Configure provider credentials only through the secret store, then install a reviewed provider adapter.

The Bank of India QR asset is unchanged. Payment confirmation remains a submission for OS verification; it is not automatic bank/payment-gateway confirmation.

## Remaining infrastructure work

- Provision SQL Server, a restricted file-storage volume, server/service account, DNS, reverse proxy, TLS certificate, monitoring, and log retention.
- Configure production CORS origins, JWT secret, database connection, storage path, and backup jobs through the hosting platform’s secret/configuration system.
- Complete DLT registration/template approval and SMS provider integration.
- Define the approved bank reconciliation process for UTR/payment receipt verification.
