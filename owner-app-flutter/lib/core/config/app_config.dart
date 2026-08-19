enum AppEnvironment {
  development,
  staging,
  production,
}

class AppConfig {
  const AppConfig({
    required this.environment,
    required this.apiBaseUrl,
    required this.appName,
  });

  final AppEnvironment environment;
  final String apiBaseUrl;
  final String appName;

  static AppConfig fromEnvironment() {
    const envName = String.fromEnvironment('ENV', defaultValue: 'development');
    switch (envName) {
      case 'staging':
        return const AppConfig(
          environment: AppEnvironment.staging,
          apiBaseUrl: 'https://mytriplea.co.za',
          appName: 'Triple A (Staging)',
        );
      case 'production':
        return const AppConfig(
          environment: AppEnvironment.production,
          apiBaseUrl: 'https://mytriplea.co.za',
          appName: 'Triple A',
        );
      case 'development':
      default:
        return const AppConfig(
          environment: AppEnvironment.development,
          apiBaseUrl: 'https://localhost:7112',
          appName: 'Triple A (Dev)',
        );
    }
  }
}
