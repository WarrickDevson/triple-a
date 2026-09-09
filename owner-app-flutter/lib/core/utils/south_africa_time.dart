/// South Africa Standard Time (SAST) Utilities.
/// South Africa operates at UTC+2 year-round without Daylight Saving Time.
class SouthAfricaTime {
  const SouthAfricaTime._();

  /// Fixed offset of UTC+2.
  static const Duration offset = Duration(hours: 2);
  static const String timeZoneName = 'SAST';
  static const String timeZoneId = 'Africa/Johannesburg';

  /// Converts any DateTime (UTC or local) to a DateTime representing
  /// the exact wall-clock date and time in South Africa Standard Time.
  static DateTime toSouthAfricaTime(DateTime dateTime) {
    final utc = dateTime.toUtc();
    final sast = utc.add(offset);
    return DateTime(
      sast.year,
      sast.month,
      sast.day,
      sast.hour,
      sast.minute,
      sast.second,
      sast.millisecond,
    );
  }

  /// Converts a South Africa wall-clock DateTime (e.g. chosen from a picker)
  /// into a UTC DateTime suitable for backend transmission.
  static DateTime fromSouthAfricaTimeToUtc(DateTime sastDateTime) {
    return DateTime.utc(
      sastDateTime.year,
      sastDateTime.month,
      sastDateTime.day,
      sastDateTime.hour,
      sastDateTime.minute,
      sastDateTime.second,
    ).subtract(offset);
  }

  /// Returns the current date and time in South Africa Standard Time.
  static DateTime now() {
    return toSouthAfricaTime(DateTime.now());
  }

  /// Formats a DateTime to a clean SAST date string: YYYY-MM-DD.
  static String toDateString(DateTime dateTime) {
    final sa = toSouthAfricaTime(dateTime);
    final y = sa.year.toString().padLeft(4, '0');
    final m = sa.month.toString().padLeft(2, '0');
    final d = sa.day.toString().padLeft(2, '0');
    return '$y-$m-$d';
  }

  /// Checks if two DateTimes fall on the same calendar day in South Africa Standard Time.
  static bool isSameDay(DateTime a, DateTime b) {
    final saA = toSouthAfricaTime(a);
    final saB = toSouthAfricaTime(b);
    return saA.year == saB.year && saA.month == saB.month && saA.day == saB.day;
  }
}
