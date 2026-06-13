import { SpApiConfig } from '../types';

function requireEnv(name: string): string {
  const value = process.env[name];
  if (!value) {
    throw new Error(`Missing required environment variable: ${name}`);
  }
  return value;
}

export function getSpApiConfig(): SpApiConfig {
  return {
    clientId: requireEnv('AMAZON_SP_API_CLIENT_ID'),
    clientSecret: requireEnv('AMAZON_SP_API_CLIENT_SECRET'),
    refreshToken: requireEnv('AMAZON_REFRESH_TOKEN'),
    awsAccessKeyId: requireEnv('AMAZON_AWS_ACCESS_KEY_ID'),
    awsSecretAccessKey: requireEnv('AMAZON_AWS_SECRET_ACCESS_KEY'),
    awsRoleArn: requireEnv('AMAZON_AWS_ROLE_ARN'),
    marketplaceId: process.env.AMAZON_MARKETPLACE_ID || 'A1PA6795UKMFR9',
  };
}

export const PORT = parseInt(process.env.PORT || '3000', 10);
