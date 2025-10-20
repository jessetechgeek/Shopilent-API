# Production Deployment Guide

This guide provides comprehensive instructions for deploying Shopilent to production using Docker Compose with Traefik reverse proxy and Let's Encrypt SSL certificates.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Domain Configuration](#domain-configuration)
- [SSL/TLS Configuration](#ssltls-configuration)
- [Service Configuration](#service-configuration)
- [Security Checklist](#security-checklist)
- [Deployment Checklist](#deployment-checklist)
- [Post-Deployment](#post-deployment)

## Prerequisites

1. **Server Requirements:**
   - Linux server with Docker and Docker Compose installed
   - Minimum 2GB RAM (4GB+ recommended)
   - 20GB+ disk space
   - Public IP address

2. **Network Requirements:**
   - Ports 80 and 443 open and accessible from the internet
   - Firewall configured to allow HTTP/HTTPS traffic
   - DNS management access for your domain

3. **Domain Requirements:**
   - Registered domain name
   - Ability to create DNS A/AAAA records
   - Domain must be publicly accessible

## Domain Configuration

### Subdomains Setup

All services will be accessible via subdomains of your main domain. Configure DNS A records for the following subdomains:

| Subdomain | Service | Description |
|-----------|---------|-------------|
| `api.yourdomain.com` | API | Main REST API |
| `admin.yourdomain.com` | Admin Panel | React admin dashboard |
| `shop.yourdomain.com` | Customer App | Next.js customer application |
| `minio.yourdomain.com` | MinIO Console | Object storage web interface |
| `s3.yourdomain.com` | S3 API | S3-compatible API endpoint |
| `logs.yourdomain.com` | Seq | Centralized logging dashboard |
| `traefik.yourdomain.com` | Traefik Dashboard | Reverse proxy dashboard (optional) |

### DNS Configuration Example

Point all subdomains to your server's public IP address:

```
Type    Name     Value
A       api      123.45.67.89
A       admin    123.45.67.89
A       shop     123.45.67.89
A       minio    123.45.67.89
A       s3       123.45.67.89
A       logs     123.45.67.89
A       traefik  123.45.67.89
```

## SSL/TLS Configuration

### Let's Encrypt Setup

The production deployment uses Let's Encrypt for automatic SSL certificate generation and renewal.

1. **Email Configuration:**
   - Set `LETSENCRYPT_EMAIL` to a valid email address
   - This email will receive certificate expiration notifications
   - Let's Encrypt will use this for important security updates

2. **Certificate Issuance:**
   - Certificates are issued automatically on first deployment
   - Traefik handles ACME challenge automatically
   - Certificates are stored in Docker volume `letsencrypt`

3. **Requirements for Let's Encrypt:**
   - Domain must be publicly accessible
   - DNS A/AAAA records must point to your server
   - Ports 80/443 must not be blocked by firewall
   - Server must be reachable from the internet

## Service Configuration

### Database - PostgreSQL

PostgreSQL is used as the primary database for all application data.

**Configuration:**
- `POSTGRES_USER`: Database username (use unique value for production)
- `POSTGRES_PASSWORD`: Strong password (minimum 32 characters recommended)
- `POSTGRES_DB`: Database name

**Security:**
- Not exposed to internet (internal Docker network only)
- Use strong, unique passwords
- Regular backups are critical (see backup section)

### Cache - Redis

Redis provides distributed caching for improved performance.

**Configuration:**
- Uses default Redis settings
- No password required if on internal network only
- Add password if exposed: `redis://:password@redis:6379`

**Security:**
- Only accessible within Docker network
- Consider adding password for additional security

### Object Storage - MinIO (S3 Compatible)

MinIO provides S3-compatible object storage for media files.

**Configuration:**
- `MINIO_ROOT_USER`: Admin username for MinIO
- `MINIO_ROOT_PASSWORD`: Strong password (minimum 32 characters)
- `S3_BUCKET_NAME`: Bucket name for storing media (default: `shopilent-media`)
- `S3_REGION`: AWS region format (default: `us-east-1`)
- `S3_PROVIDER`: Set to `MinIO`

**Endpoints:**
- Console: `https://minio.yourdomain.com` (web interface)
- S3 API: `https://s3.yourdomain.com` (API endpoint)

**Security:**
- Change default credentials
- Use strong passwords
- Regularly audit bucket policies

### Search - Meilisearch

Meilisearch provides fast full-text search capabilities.

**Configuration:**
- `MEILISEARCH_MASTER_KEY`: Master key (minimum 16 characters required)

**Security:**
- Master key required for all operations
- Not exposed externally (internal network only)
- Keep master key secure

### Logging - Seq

Seq provides centralized structured logging with a web interface.

**Configuration:**
- `SEQ_ADMIN_USERNAME`: Admin username (default: `admin`)
- `SEQ_ADMIN_PASSWORD`: Strong password (minimum 16 characters)
- `SEQ_BASIC_AUTH`: Optional HTTP basic auth (username:$$hash format)

**Accessing Seq:**
- URL: `https://logs.yourdomain.com`
- Use admin credentials to log in

**Generating Basic Auth Hash:**
```bash
docker run --rm httpd:alpine htpasswd -nb admin your-password
```

### Email - SMTP Configuration

Configure your production email service for sending transactional emails.

**Supported Providers:**

1. **Gmail:**
   ```env
   SMTP_SERVER=smtp.gmail.com
   SMTP_PORT=587
   SMTP_USERNAME=your-email@gmail.com
   SMTP_PASSWORD=your-app-specific-password
   SMTP_USE_SSL=true
   ```

2. **SendGrid:**
   ```env
   SMTP_SERVER=smtp.sendgrid.net
   SMTP_PORT=587
   SMTP_USERNAME=apikey
   SMTP_PASSWORD=your-sendgrid-api-key
   SMTP_USE_SSL=true
   ```

3. **AWS SES:**
   ```env
   SMTP_SERVER=email-smtp.us-east-1.amazonaws.com
   SMTP_PORT=587
   SMTP_USERNAME=your-ses-smtp-username
   SMTP_PASSWORD=your-ses-smtp-password
   SMTP_USE_SSL=true
   ```

**Email Settings:**
- `SMTP_FROM_EMAIL`: Sender email address (e.g., `noreply@yourdomain.com`)
- `SMTP_FROM_NAME`: Sender display name (e.g., `Shopilent Store`)

### JWT Authentication

JWT tokens are used for API authentication and authorization.

**Configuration:**
- `JWT_SECRET`: Strong random string (minimum 64 characters recommended)
- `JWT_ISSUER`: API URL (e.g., `https://api.yourdomain.com`)
- `JWT_AUDIENCE`: API URL (same as issuer)
- `JWT_ACCESS_TOKEN_LIFETIME`: Access token lifetime in minutes (default: 15)
- `JWT_REFRESH_TOKEN_LIFETIME`: Refresh token lifetime in days (default: 7)

**Generating JWT Secret:**
```bash
openssl rand -base64 64
```

**Security:**
- Use very long random strings
- Never reuse secrets across environments
- Keep secrets secure and rotate regularly

### Payment - Stripe

Stripe integration for payment processing.

**Configuration:**
- `STRIPE_SECRET_KEY`: Production secret key (starts with `sk_live_`)
- `STRIPE_PUBLISHABLE_KEY`: Production publishable key (starts with `pk_live_`)
- `STRIPE_WEBHOOK_SECRET`: Webhook signing secret (starts with `whsec_`)

**Getting Keys:**
1. Log in to [Stripe Dashboard](https://dashboard.stripe.com/apikeys)
2. Use production (live) keys, not test keys
3. Configure webhook endpoint: `https://api.yourdomain.com/api/v1/webhooks/stripe`

### CORS Configuration

CORS (Cross-Origin Resource Sharing) is configured in `appsettings.json` or `appsettings.Production.json`, not via environment variables.

**Configuration Location:** `src/API/Shopilent.API/appsettings.Production.json`

**Example:**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://admin.yourdomain.com",
      "https://shop.yourdomain.com",
      "https://www.yourdomain.com"
    ]
  }
}
```

**Note:** Update the `AllowedOrigins` array in your production appsettings file to include all frontend domains that should be allowed to make API requests.

### Traefik Dashboard (Optional)

The Traefik dashboard provides monitoring and debugging capabilities.

**Configuration:**
- `TRAEFIK_DASHBOARD_AUTH`: HTTP basic auth (username:$$hash format)

**Generating Auth Hash:**
```bash
echo $(htpasswd -nb admin your-password) | sed -e s/\\$/\\$\\$/g
```

Or using Docker:
```bash
docker run --rm httpd:alpine htpasswd -nb admin your-password
```

**Format:** `admin:$$apr1$$hash$$moreHash`

## Security Checklist

Before deploying to production, ensure you have completed these security steps:

### Passwords and Secrets

- [ ] All passwords changed to strong, unique values (32+ characters)
- [ ] JWT_SECRET generated with cryptographically secure random data
- [ ] No default or example passwords remain
- [ ] All secrets stored securely (use password manager)

### Database Security

- [ ] PostgreSQL password is strong and unique
- [ ] Database not exposed to public internet
- [ ] Regular backup strategy implemented
- [ ] Backup restoration tested

### Network Security

- [ ] Firewall configured to allow only ports 80 and 443
- [ ] Internal services not exposed externally
- [ ] Rate limiting configured if needed
- [ ] DDoS protection considered

### SSL/TLS

- [ ] Domain DNS properly configured
- [ ] SSL certificates issued successfully
- [ ] HTTPS enforced for all services
- [ ] HTTP redirects to HTTPS working

### Application Security

- [ ] CORS origins properly configured
- [ ] Security headers reviewed and applied
- [ ] API authentication working correctly
- [ ] Role-based authorization tested

### Monitoring and Logging

- [ ] Seq logging configured and accessible
- [ ] Log retention policies defined
- [ ] Monitoring alerts configured
- [ ] Error tracking in place

### File Storage

- [ ] MinIO credentials changed from defaults
- [ ] Bucket policies reviewed
- [ ] File upload limits configured
- [ ] Storage quotas considered

### Email

- [ ] SMTP credentials configured
- [ ] Email sending tested
- [ ] SPF/DKIM records configured (if using custom domain)
- [ ] Email deliverability verified

## Deployment Checklist

Complete these steps before and after deployment:

### Pre-Deployment

- [ ] Copy `.env.production.example` to `.env.production`
- [ ] Update `DOMAIN` with your actual domain
- [ ] Update `LETSENCRYPT_EMAIL` with valid email
- [ ] Change all passwords to strong values
- [ ] Generate new JWT_SECRET (64+ characters)
- [ ] Configure SMTP settings for email
- [ ] Add Stripe production keys
- [ ] Generate Traefik dashboard auth hash
- [ ] Configure DNS A records for all subdomains
- [ ] Ensure ports 80/443 are open
- [ ] Review security settings

### Deployment

- [ ] Pull latest code from repository
- [ ] Build Docker images
- [ ] Run database migrations
- [ ] Start services with `docker-compose -f compose.prod.yaml up -d`
- [ ] Monitor logs for errors
- [ ] Verify all services are running

### Post-Deployment

- [ ] Test SSL certificate issuance
- [ ] Verify all subdomains are accessible
- [ ] Test API endpoints
- [ ] Test admin panel login
- [ ] Test customer app functionality
- [ ] Verify email sending works
- [ ] Test payment processing (small test transaction)
- [ ] Check Seq logs for errors
- [ ] Monitor resource usage
- [ ] Set up database backup automation
- [ ] Configure monitoring and alerting
- [ ] Document any custom configurations

## Post-Deployment

### Accessing Services

After successful deployment, your services will be available at:

- **API Documentation:** `https://api.yourdomain.com/api-docs`
- **Admin Panel:** `https://admin.yourdomain.com`
- **Customer App:** `https://shop.yourdomain.com`
- **MinIO Console:** `https://minio.yourdomain.com`
- **Logs Dashboard:** `https://logs.yourdomain.com`
- **Traefik Dashboard:** `https://traefik.yourdomain.com` (if enabled)

### Monitoring

1. **Logs:**
   - Access Seq at `https://logs.yourdomain.com`
   - Use admin credentials from `.env.production`
   - Set up saved queries for common issues
   - Configure alerts for errors

2. **Health Checks:**
   - API health: `https://api.yourdomain.com/health`
   - Monitor response times and availability

3. **Resource Monitoring:**
   - Use `docker stats` to monitor container resources
   - Set up external monitoring (UptimeRobot, Pingdom, etc.)

### Backup Strategy

**Critical Data to Backup:**
- PostgreSQL database (volume: `postgres_data`)
- MinIO object storage (volume: `minio_data`)
- Let's Encrypt certificates (volume: `letsencrypt`)

**Backup Commands:**

```bash
# Backup PostgreSQL database
docker exec postgres pg_dump -U shopilent_prod shopilent_production > backup_$(date +%Y%m%d).sql

# Backup MinIO data (use MinIO client)
mc mirror minio/shopilent-media ./backup/minio/

# Backup entire volume
docker run --rm -v postgres_data:/data -v $(pwd):/backup alpine tar czf /backup/postgres_backup.tar.gz -C /data .
```

**Backup Schedule:**
- Daily automated backups
- Weekly full backups
- Monthly retention of full backups
- Test restoration regularly

### Maintenance

**Regular Tasks:**
- Monitor disk space usage
- Review logs for errors or security issues
- Update Docker images for security patches
- Rotate SSL certificates (automatic with Let's Encrypt)
- Review and optimize database performance
- Clean up old logs and backups

**Updates:**
```bash
# Pull latest images
docker-compose -f compose.prod.yaml pull

# Restart services with new images
docker-compose -f compose.prod.yaml up -d

# Clean up old images
docker image prune -a
```

### Troubleshooting

**SSL Certificate Issues:**
- Verify DNS records point to correct IP
- Check ports 80/443 are accessible
- Review Traefik logs: `docker-compose -f compose.prod.yaml logs traefik`
- Wait up to 10 minutes for certificate issuance

**Service Not Accessible:**
- Check service status: `docker-compose -f compose.prod.yaml ps`
- Review logs: `docker-compose -f compose.prod.yaml logs [service-name]`
- Verify DNS propagation
- Check firewall rules

**Database Connection Issues:**
- Verify PostgreSQL is running
- Check connection string in .env.production
- Review database logs: `docker-compose -f compose.prod.yaml logs postgres`

**Email Not Sending:**
- Verify SMTP credentials
- Check SMTP server allows connections
- Review API logs in Seq
- Test SMTP connection manually

## Support and Resources

- **Docker Documentation:** https://docs.docker.com
- **Traefik Documentation:** https://doc.traefik.io/traefik/
- **Let's Encrypt:** https://letsencrypt.org/docs/
- **MinIO Documentation:** https://min.io/docs/
- **Stripe API:** https://stripe.com/docs/api

## Security Notes

1. **Never commit `.env.production` to version control** - Add to `.gitignore`
2. **Use strong, unique passwords** for all services
3. **Rotate secrets regularly** especially after personnel changes
4. **Monitor logs** for suspicious activity
5. **Keep Docker images updated** for security patches
6. **Implement rate limiting** to prevent abuse
7. **Regular security audits** of configuration and code
8. **Backup encryption** for sensitive data backups
9. **Secure your server** with proper firewall rules
10. **Use Docker secrets** for additional security in production

## Additional Considerations

### Scaling

For high-traffic deployments, consider:
- Multiple API instances with load balancing
- PostgreSQL read replicas
- Redis Sentinel for high availability
- CDN for static assets
- Dedicated MinIO cluster

### Performance

- Enable HTTP/2 in Traefik
- Configure PostgreSQL connection pooling
- Optimize Redis cache expiration policies
- Use CDN for media files
- Enable compression in Traefik

### High Availability

- Multi-node Docker Swarm or Kubernetes
- Database replication and failover
- Distributed Redis cluster
- Multi-region MinIO deployment
- Health checks and automatic restarts
