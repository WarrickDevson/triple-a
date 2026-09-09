import 'package:flutter_test/flutter_test.dart';
import 'package:owner_app_flutter/core/utils/south_africa_time.dart';
import 'package:owner_app_flutter/core/utils/formatters.dart';

void main() {
  group('SouthAfricaTime', () {
    test('toSouthAfricaTime converts UTC to SAST (UTC+2)', () {
      // 2026-09-09 08:30:00 UTC should be 10:30:00 in SAST
      final utc = DateTime.utc(2026, 9, 9, 8, 30);
      final sast = SouthAfricaTime.toSouthAfricaTime(utc);

      expect(sast.year, 2026);
      expect(sast.month, 9);
      expect(sast.day, 9);
      expect(sast.hour, 10);
      expect(sast.minute, 30);
    });

    test('toSouthAfricaTime handles date crossover at midnight', () {
      // 2026-09-09 23:30:00 UTC should be 2026-09-10 01:30:00 in SAST
      final utc = DateTime.utc(2026, 9, 9, 23, 30);
      final sast = SouthAfricaTime.toSouthAfricaTime(utc);

      expect(sast.day, 10);
      expect(sast.hour, 1);
      expect(sast.minute, 30);
    });

    test('fromSouthAfricaTimeToUtc converts SAST (UTC+2) to UTC', () {
      // User selects 10:30 on 2026-09-09 in SAST -> 08:30 UTC
      final sastWallClock = DateTime(2026, 9, 9, 10, 30);
      final utc = SouthAfricaTime.fromSouthAfricaTimeToUtc(sastWallClock);

      expect(utc.isUtc, true);
      expect(utc.year, 2026);
      expect(utc.month, 9);
      expect(utc.day, 9);
      expect(utc.hour, 8);
      expect(utc.minute, 30);
    });

    test('isSameDay correctly identifies same day in SAST', () {
      final a = DateTime.utc(2026, 9, 9, 8, 0); // 10:00 SAST
      final b = DateTime.utc(2026, 9, 9, 21, 0); // 23:00 SAST
      final c = DateTime.utc(2026, 9, 9, 22, 30); // 00:30 next day SAST

      expect(SouthAfricaTime.isSameDay(a, b), isTrue);
      expect(SouthAfricaTime.isSameDay(a, c), isFalse);
    });

    test('toDateString formats yyyy-MM-dd in SAST', () {
      final utc = DateTime.utc(2026, 9, 9, 23, 0); // 2026-09-10 in SAST
      expect(SouthAfricaTime.toDateString(utc), '2026-09-10');
    });
  });

  group('formatters with SouthAfricaTime', () {
    test('formatAppointmentTime formats SAST correctly', () {
      final utc = DateTime.utc(2026, 9, 9, 8, 0); // 10:00 SAST
      expect(formatAppointmentTime(utc), '10:00 AM');

      final utcAfternoon = DateTime.utc(2026, 9, 9, 12, 30); // 14:30 SAST
      expect(formatAppointmentTime(utcAfternoon), '2:30 PM');
    });

    test('formatAppointmentDate formats SAST correctly', () {
      final utc = DateTime.utc(2026, 9, 9, 22, 30); // Sep 10 in SAST
      expect(formatAppointmentDate(utc), 'Sep 10, 2026');
    });
  });
}
