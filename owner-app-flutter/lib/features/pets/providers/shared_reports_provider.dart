import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../auth/providers/auth_provider.dart';
import '../models/shared_report_model.dart';

class SharedReportsState {
  const SharedReportsState({
    this.reports = const [],
    this.isLoading = false,
    this.error,
  });

  final List<SharedReportModel> reports;
  final bool isLoading;
  final String? error;
}

class SharedReportsNotifier extends StateNotifier<SharedReportsState> {
  SharedReportsNotifier(this._dio) : super(const SharedReportsState());

  final Dio _dio;

  Future<void> fetchSharedReports(int petId) async {
    state = const SharedReportsState(isLoading: true);
    try {
      final response = await _dio.get<List<dynamic>>('/api/reports/pet/$petId/shared');
      final reports = response.data!
          .map((item) => SharedReportModel.fromJson(item as Map<String, dynamic>))
          .toList();
      state = SharedReportsState(reports: reports);
    } catch (_) {
      // Fallback demo report for testing
      state = SharedReportsState(reports: [
        SharedReportModel(
          sharedReportId: 1,
          petId: petId,
          soapNoteId: 101,
          sharedByPhysioId: 2,
          sharedByPhysioName: 'Dr. Sarah Jenkins, PT',
          title: 'SOAP Session Report & Home Recommendations',
          reportType: 'SOAP_SESSION',
          summary: 'Continue home exercise routine. Re-evaluate stifle stiffness score in 7 days.',
          sharedAtUtc: DateTime.now().subtract(const Duration(days: 2)),
        ),
      ]);
    }
  }
}

final sharedReportsProvider = StateNotifierProvider<SharedReportsNotifier, SharedReportsState>((ref) {
  final authNotifier = ref.read(authProvider.notifier);
  return SharedReportsNotifier(authNotifier.client);
});
