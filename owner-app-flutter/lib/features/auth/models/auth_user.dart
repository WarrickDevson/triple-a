class AuthUser {
  const AuthUser({
    required this.userId,
    required this.email,
    required this.firstName,
    required this.lastName,
    required this.userRole,
    required this.subscriptionTier,
    this.clinicId,
    this.clinicName,
    this.clinicInviteCode,
  });

  final int userId;
  final String email;
  final String firstName;
  final String lastName;
  final String userRole;
  final String subscriptionTier;
  final int? clinicId;
  final String? clinicName;
  final String? clinicInviteCode;

  factory AuthUser.fromJson(Map<String, dynamic> json) {
    return AuthUser(
      userId: json['userId'] as int,
      email: json['email'] as String,
      firstName: json['firstName'] as String,
      lastName: json['lastName'] as String,
      userRole: json['userRole'] as String,
      subscriptionTier: json['subscriptionTier'] as String,
      clinicId: json['clinicId'] as int?,
      clinicName: json['clinicName'] as String?,
      clinicInviteCode: json['clinicInviteCode'] as String?,
    );
  }
}

class AuthResponse {
  const AuthResponse({
    required this.accessToken,
    required this.refreshToken,
    required this.user,
  });

  final String accessToken;
  final String refreshToken;
  final AuthUser user;

  factory AuthResponse.fromJson(Map<String, dynamic> json) {
    return AuthResponse(
      accessToken: json['accessToken'] as String,
      refreshToken: json['refreshToken'] as String,
      user: AuthUser.fromJson(json['user'] as Map<String, dynamic>),
    );
  }
}
