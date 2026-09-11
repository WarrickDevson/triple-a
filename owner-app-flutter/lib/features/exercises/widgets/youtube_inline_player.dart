import 'package:flutter/widgets.dart';

import 'youtube_inline_player_stub.dart'
    if (dart.library.js_interop) 'youtube_inline_player_web.dart'
    as player_impl;

Widget getYouTubeInlinePlayer({
  required String youTubeId,
}) {
  return player_impl.buildYouTubeInlinePlayer(youTubeId: youTubeId);
}
