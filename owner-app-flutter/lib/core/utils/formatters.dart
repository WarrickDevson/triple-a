import 'south_africa_time.dart';

String? formatPetAge(String? birthDate) {
  if (birthDate == null || birthDate.isEmpty) return null;
  try {
    final born = DateTime.parse(birthDate);
    final now = SouthAfricaTime.now();
    var years = now.year - born.year;
    if (now.month < born.month || (now.month == born.month && now.day < born.day)) {
      years--;
    }
    if (years < 1) {
      var months = (now.year - born.year) * 12 + now.month - born.month;
      if (now.day < born.day) months--;
      return months <= 1 ? '1 mo' : '$months mos';
    }
    return years == 1 ? '1 yr' : '$years yrs';
  } catch (_) {
    return null;
  }
}

String formatPetSubtitle({String? breed, String? birthDate}) {
  final parts = <String>[];
  if (breed != null && breed.isNotEmpty) parts.add(breed);
  final age = formatPetAge(birthDate);
  if (age != null) parts.add(age);
  return parts.join(' · ');
}

/// Placeholder weekly progress until tracking API exposes aggregates.
double placeholderWeeklyProgress(int petId) => 55 + (petId % 4) * 8.0;

String formatRelativeTime(DateTime dateTime) {
  final nowUtc = DateTime.now().toUtc();
  final dtUtc = dateTime.toUtc();
  final diff = nowUtc.difference(dtUtc);
  if (diff.inMinutes < 1) return 'Just now';
  if (diff.inMinutes < 60) return '${diff.inMinutes}m ago';
  if (diff.inHours < 24) return '${diff.inHours}h ago';
  if (diff.inDays < 7) return '${diff.inDays}d ago';
  final sa = SouthAfricaTime.toSouthAfricaTime(dateTime);
  return '${sa.day}/${sa.month}/${sa.year}';
}

String formatAppointmentTime(DateTime dateTime) {
  final sa = SouthAfricaTime.toSouthAfricaTime(dateTime);
  final hour = sa.hour > 12 ? sa.hour - 12 : (sa.hour == 0 ? 12 : sa.hour);
  final minute = sa.minute.toString().padLeft(2, '0');
  final period = sa.hour >= 12 ? 'PM' : 'AM';
  return '$hour:$minute $period';
}

String formatAppointmentDate(DateTime dateTime) {
  final sa = SouthAfricaTime.toSouthAfricaTime(dateTime);
  const months = [
    'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
    'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec',
  ];
  return '${months[sa.month - 1]} ${sa.day}, ${sa.year}';
}

