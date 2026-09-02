import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/widgets/app_bottom_nav.dart';
import '../../core/widgets/unverified_account_banner.dart';
import '../dashboard/screens/home_screen.dart';
import '../messages/screens/messages_screen.dart';
import '../more/screens/more_screen.dart';
import '../pets/screens/pets_list_screen.dart';

class MainShell extends ConsumerStatefulWidget {
  const MainShell({
    super.key,
    this.initialTab = 0,
    this.messagesPetId,
    this.messagesInitialText,
    this.messagesVideoSubmissionId,
  });

  final int initialTab;
  final int? messagesPetId;
  final String? messagesInitialText;
  final int? messagesVideoSubmissionId;

  @override
  ConsumerState<MainShell> createState() => _MainShellState();
}

class _MainShellState extends ConsumerState<MainShell> {
  late int _currentIndex;

  @override
  void initState() {
    super.initState();
    _currentIndex = widget.initialTab;
  }

  @override
  Widget build(BuildContext context) {
    final screens = [
      const HomeScreen(embedded: true),
      const PetsListScreen(embedded: true),
      MessagesScreen(
        embedded: true,
        initialPetId: widget.messagesPetId,
        initialMessage: widget.messagesInitialText,
        initialVideoSubmissionId: widget.messagesVideoSubmissionId,
        openThreadOnLoad: widget.messagesPetId != null,
      ),
      const MoreScreen(),
    ];

    return Scaffold(
      backgroundColor: Colors.transparent,
      body: Column(
        children: [
          const UnverifiedAccountBanner(),
          Expanded(
            child: IndexedStack(
              index: _currentIndex,
              children: List.generate(screens.length, (index) {
                return ExcludeSemantics(
                  excluding: index != _currentIndex,
                  child: screens[index],
                );
              }),
            ),
          ),
        ],
      ),
      bottomNavigationBar: MediaQuery.of(context).viewInsets.bottom > 0
          ? null
          : AppBottomNav(
              currentIndex: _currentIndex,
              onTap: (index) => setState(() => _currentIndex = index),
            ),
    );
  }
}
