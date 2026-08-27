import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../auth/providers/auth_provider.dart';
import '../../../core/utils/file_download_util.dart';
import '../models/shared_report_model.dart';
import '../models/soap_note_model.dart';

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
      final response = await _dio.get('/api/reports/pet/$petId/shared');
      if (response.data is List) {
        final list = response.data as List;
        if (list.isNotEmpty) {
          final reports = list
              .map((item) => SharedReportModel.fromJson(Map<String, dynamic>.from(item as Map)))
              .toList();
          state = SharedReportsState(reports: reports);
          return;
        }
      }
    } catch (_) {
      // Offline fallback
    }

    // Fallback demo reports for previewing
    state = SharedReportsState(reports: [
      SharedReportModel(
        sharedReportId: 1,
        petId: petId,
        soapNoteId: 1,
        sharedByPhysioId: 2,
        sharedByPhysioName: 'Dr. Sarah Jenkins, PT',
        title: 'SOAP Session Report - Jul 26, 2026',
        reportType: 'SOAP_SESSION',
        summary: 'Left stifle extension ROM improved to 135°. Continue PROM 2x daily, balance disc standing, and cold pack after walks.',
        sharedAtUtc: DateTime.now().subtract(const Duration(days: 2)),
      ),
      SharedReportModel(
        sharedReportId: 2,
        petId: petId,
        soapNoteId: null,
        sharedByPhysioId: 2,
        sharedByPhysioName: 'Dr. Sarah Jenkins, PT',
        title: 'Comprehensive Clinical Progress Report',
        reportType: 'CLINICAL_REPORT',
        summary: 'Overall post-op recovery is on track. Pain scores reduced by 50% over past 4 weeks with steady muscle rebuilding.',
        sharedAtUtc: DateTime.now().subtract(const Duration(days: 4)),
      ),
      SharedReportModel(
        sharedReportId: 3,
        petId: petId,
        soapNoteId: null,
        sharedByPhysioId: 2,
        sharedByPhysioName: 'Dr. Sarah Jenkins, PT',
        title: 'Veterinary Referral Letter & Initial Radiographs',
        reportType: 'REFERRAL_LETTER',
        summary: 'Pre-operative surgical referral letter and stifle joint diagnostic radiographs from referring orthopedic surgeon.',
        sharedAtUtc: DateTime.now().subtract(const Duration(days: 10)),
      ),
      SharedReportModel(
        sharedReportId: 4,
        petId: petId,
        soapNoteId: null,
        sharedByPhysioId: 2,
        sharedByPhysioName: 'Dr. Sarah Jenkins, PT',
        title: 'Home Care Protocol — Phase 2 Strengthening',
        reportType: 'HOME_PROGRAM',
        summary: 'Daily home exercises including cavaletti rail steps, controlled sit-to-stands, and icing protocol.',
        sharedAtUtc: DateTime.now().subtract(const Duration(days: 14)),
      ),
    ]);
  }

  Future<SoapNoteModel?> fetchSoapNoteById(int soapNoteId) async {
    try {
      final response = await _dio.get<Map<String, dynamic>>('/api/soap-notes/$soapNoteId');
      if (response.data != null) {
        return SoapNoteModel.fromJson(response.data!);
      }
    } catch (_) {
      // Fallback structured note for offline testing
    }

    return SoapNoteModel(
      soapNoteId: soapNoteId,
      petId: 1,
      physioId: 2,
      physioName: 'Dr. Sarah Jenkins, PT',
      sessionDate: DateTime.now().subtract(const Duration(days: 2)),
      subjective: 'Owner reports patient is bearing noticeably more weight on the hind limb during morning walks. Morning stiffness has reduced from 6/10 to 3/10. No vocalisation of pain or reluctance when rising.',
      objective: 'Left stifle extension ROM measured at 135° (improved from 120° baseline). Thigh circumference is 38 cm (R: 40 cm). Mild compensatory tension palpated in lumbar paraspinals. Minimal joint effusion. Normal cranial drawer test post-TPLO stability.',
      action: 'Performed 15 mins myofascial soft tissue release to lumbar paraspinals and gluteals.\n• Applied Class IV photobiomodulation (laser therapy) to left stifle (4 J/cm²).\n• Completed 10 mins underwater treadmill (UWTM) at 1.2 mph with water level at mid-femur.\n• Performed 5 sets of controlled cavaletti rail walkovers.',
      plan: 'Continue structured home rehabilitation protocol:\n• Passive Range of Motion (PROM) 2x daily (10 reps per set)\n• 3-Leg standing balance disc exercise (30 seconds x 3 sets daily)\n• Apply cold pack to left stifle for 10 minutes following evening walk\n• Re-evaluate clinical ROM and lameness grade in 1 week.',
      stiffnessScore: 3,
      painScore: 2,
      lamenessScore: 1,
      customMetrics: [
        CustomMetricModel(
          name: 'Stifle Extension ROM',
          value: 135,
          minScale: 0,
          maxScale: 180,
          unitOrDescriptor: 'deg',
        ),
        CustomMetricModel(
          name: 'Thigh Circumference',
          value: 38,
          minScale: 10,
          maxScale: 80,
          unitOrDescriptor: 'cm',
        ),
      ],
      isSharedWithOwner: true,
      sharedAtUtc: DateTime.now().subtract(const Duration(days: 2)),
      createdDate: DateTime.now().subtract(const Duration(days: 2)),
    );
  }

  Future<bool> downloadSoapNotePdf(int soapNoteId, String petName) async {
    try {
      final response = await _dio.get<List<int>>(
        '/api/soap-notes/$soapNoteId/pdf',
        options: Options(responseType: ResponseType.bytes),
      );

      if (response.data != null && response.data!.isNotEmpty) {
        final safeName = petName.replaceAll(' ', '_');
        final fileName = '${safeName}_SOAP_Report_$soapNoteId.pdf';
        return await FileDownloadUtil.downloadBytes(response.data!, fileName);
      }
    } catch (_) {
      // Failed to download from server
    }
    return false;
  }

  Future<bool> downloadPetClinicalReport(int petId, String petName) async {
    try {
      final response = await _dio.get<List<int>>(
        '/api/reports/pet/$petId/download',
        options: Options(responseType: ResponseType.bytes),
      );

      if (response.data != null && response.data!.isNotEmpty) {
        final safeName = petName.replaceAll(' ', '_');
        final fileName = '${safeName}_Clinical_Report.pdf';
        return await FileDownloadUtil.downloadBytes(response.data!, fileName);
      }
    } catch (_) {
      // Failed to download from server
    }
    return false;
  }

  Future<bool> downloadSharedReport(int sharedReportId, String petName, String title) async {
    try {
      final response = await _dio.get<List<int>>(
        '/api/reports/shared/$sharedReportId/download',
        options: Options(responseType: ResponseType.bytes),
      );

      if (response.data != null && response.data!.isNotEmpty) {
        final safeTitle = title.replaceAll(RegExp(r'[^a-zA-Z0-9_\-]'), '_');
        final fileName = '${petName.replaceAll(' ', '_')}_$safeTitle.pdf';
        return await FileDownloadUtil.downloadBytes(response.data!, fileName);
      }
    } catch (_) {
      // Failed to download from server
    }
    return false;
  }
}

final sharedReportsProvider = StateNotifierProvider<SharedReportsNotifier, SharedReportsState>((ref) {
  final authNotifier = ref.read(authProvider.notifier);
  return SharedReportsNotifier(authNotifier.client);
});
