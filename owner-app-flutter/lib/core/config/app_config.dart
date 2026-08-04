import 'package:flutter/foundation.dart';

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
    const customBaseUrl = String.fromEnvironment('API_BASE_URL', defaultValue: '');

    switch (envName) {
      case 'staging':
        return AppConfig(
          environment: AppEnvironment.staging,
          apiBaseUrl: customBaseUrl.isNotEmpty ? customBaseUrl : 'https://kpw.devson.co.za',
          appName: 'Triple A (Staging)',
        );
      case 'production':
        return AppConfig(
          environment: AppEnvironment.production,
          apiBaseUrl: customBaseUrl.isNotEmpty ? customBaseUrl : 'https://api.kpw.movewell',
          appName: 'Triple A',
        );
      case 'development':
      default:
        String defaultDevUrl = 'http://localhost:5057';
        if (!kIsWeb && defaultTargetPlatform == TargetPlatform.android) {
          defaultDevUrl = 'http://10.0.2.2:5057';
        }
        return AppConfig(
          environment: AppEnvironment.development,
          apiBaseUrl: customBaseUrl.isNotEmpty ? customBaseUrl : defaultDevUrl,
          appName: 'Triple A (Dev)',
        );
    }
  }
}
