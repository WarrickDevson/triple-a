String? formatPetAge(String? birthDate) {
  if (birthDate == null || birthDate.isEmpty) return null;
  try {
    final born = DateTime.parse(birthDate);
    final now = DateTime.now();
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
  final diff = DateTime.now().difference(dateTime);
  if (diff.inMinutes < 1) return 'Just now';
  if (diff.inMinutes < 60) return '${diff.inMinutes}m ago';
  if (diff.inHours < 24) return '${diff.inHours}h ago';
  if (diff.inDays < 7) return '${diff.inDays}d ago';
  return '${dateTime.day}/${dateTime.month}/${dateTime.year}';
}

String formatAppointmentTime(DateTime dateTime) {
  final hour = dateTime.hour > 12 ? dateTime.hour - 12 : (dateTime.hour == 0 ? 12 : dateTime.hour);
  final minute = dateTime.minute.toString().padLeft(2, '0');
  final period = dateTime.hour >= 12 ? 'PM' : 'AM';
  return '$hour:$minute $period';
}

String formatAppointmentDate(DateTime dateTime) {
  const months = [
    'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
    'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec',
  ];
  return '${months[dateTime.month - 1]} ${dateTime.day}, ${dateTime.year}';
}
