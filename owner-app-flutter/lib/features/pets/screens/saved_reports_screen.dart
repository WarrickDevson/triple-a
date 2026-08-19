import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../../core/config/app_config.dart';
import '../models/pet.dart';
import '../providers/shared_reports_provider.dart';

class SavedReportsScreen extends ConsumerStatefulWidget {
  const SavedReportsScreen({super.key, required this.pet});

  final Pet pet;

  @override
  ConsumerState<SavedReportsScreen> createState() => _SavedReportsScreenState();
}

class _SavedReportsScreenState extends ConsumerState<SavedReportsScreen> {
  @override
  void initState() {
    super.initState();
    Future.microtask(() {
      ref.read(sharedReportsProvider.notifier).fetchSharedReports(widget.pet.petId);
    });
  }

  void _openPdfReport(int? soapNoteId) async {
    if (soapNoteId == null) return;
    final baseUrl = AppConfig.fromEnvironment().apiBaseUrl.replaceAll(RegExp(r'/+$'), '');
    final Uri url = Uri.parse('$baseUrl/api/soap-notes/$soapNoteId/pdf');
    if (await canLaunchUrl(url)) {
      await launchUrl(url, mode: LaunchMode.externalApplication);
    } else {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Could not download PDF report.')),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(sharedReportsProvider);

    return Scaffold(
      appBar: AppBar(
        title: Text('${widget.pet.petName}\'s Saved Reports'),
        backgroundColor: const Color(0xFF0C3C54),
        foregroundColor: Colors.white,
      ),
      body: state.isLoading
          ? const Center(child: CircularProgressIndicator())
          : state.reports.isEmpty
              ? Center(
                  child: Padding(
                    padding: const EdgeInsets.all(24.0),
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(Icons.assignment_outlined, size: 64, color: Colors.grey.shade400),
                        const SizedBox(height: 16),
                        const Text(
                          'No Saved Reports Yet',
                          style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF0C3C54)),
                        ),
                        const SizedBox(height: 8),
                        const Text(
                          'Clinical SOAP reports shared by your physio will appear here.',
                          textAlign: TextAlign.center,
                          style: TextStyle(color: Colors.grey),
                        ),
                      ],
                    ),
                  ),
                )
              : ListView.builder(
                  padding: const EdgeInsets.all(16),
                  itemCount: state.reports.length,
                  itemBuilder: (context, index) {
                    final report = state.reports[index];
                    final dateStr = '${report.sharedAtUtc.year}-${report.sharedAtUtc.month.toString().padLeft(2, '0')}-${report.sharedAtUtc.day.toString().padLeft(2, '0')}';

                    return Card(
                      margin: const EdgeInsets.only(bottom: 12),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                      elevation: 2,
                      child: Padding(
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Expanded(
                                  child: Text(
                                    report.title,
                                    style: const TextStyle(
                                      fontWeight: FontWeight.bold,
                                      fontSize: 15,
                                      color: Color(0xFF0C3C54),
                                    ),
                                  ),
                                ),
                                Container(
                                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                                  decoration: BoxDecoration(
                                    color: const Color(0xFF6B7A4D).withValues(alpha: 0.1),
                                    borderRadius: BorderRadius.circular(8),
                                  ),
                                  child: Text(
                                    dateStr,
                                    style: const TextStyle(
                                      fontSize: 11,
                                      fontWeight: FontWeight.bold,
                                      color: Color(0xFF6B7A4D),
                                    ),
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: 6),
                            Text(
                              'Shared by ${report.sharedByPhysioName}',
                              style: TextStyle(fontSize: 12, color: Colors.grey.shade600),
                            ),
                            if (report.summary != null && report.summary!.isNotEmpty) ...[
                              const SizedBox(height: 10),
                              Container(
                                width: double.infinity,
                                padding: const EdgeInsets.all(10),
                                decoration: BoxDecoration(
                                  color: Colors.grey.shade100,
                                  borderRadius: BorderRadius.circular(8),
                                ),
                                child: Text(
                                  report.summary!,
                                  style: const TextStyle(fontSize: 13, color: Color(0xFF212529)),
                                ),
                              ),
                            ],
                            const SizedBox(height: 12),
                            Align(
                              alignment: Alignment.centerRight,
                              child: ElevatedButton.icon(
                                style: ElevatedButton.styleFrom(
                                  backgroundColor: const Color(0xFF6B7A4D),
                                  foregroundColor: Colors.white,
                                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                                ),
                                onPressed: () => _openPdfReport(report.soapNoteId),
                                icon: const Icon(Icons.picture_as_pdf, size: 16),
                                label: const Text('Download PDF'),
                              ),
                            ),
                          ],
                        ),
                      ),
                    );
                  },
                ),
    );
  }
}
