import 'dart:ui_web' as ui_web;
import 'package:flutter/widgets.dart';
import 'package:web/web.dart' as web;

final Set<String> _registeredYouTubeViews = {};

Widget buildYouTubeInlinePlayer({
  required String youTubeId,
}) {
  final viewType = 'youtube-inline-$youTubeId';

  if (!_registeredYouTubeViews.contains(viewType)) {
    ui_web.platformViewRegistry.registerViewFactory(
      viewType,
      (int viewId) {
        final iframe = web.document.createElement('iframe') as web.HTMLIFrameElement;
        iframe.src = 'https://www.youtube-nocookie.com/embed/$youTubeId?autoplay=1&rel=0&modestbranding=1&playsinline=1';
        iframe.style.border = 'none';
        iframe.style.width = '100%';
        iframe.style.height = '100%';
        iframe.setAttribute('allow', 'accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share');
        iframe.setAttribute('allowfullscreen', 'true');
        return iframe;
      },
    );
    _registeredYouTubeViews.add(viewType);
  }

  return HtmlElementView(viewType: viewType);
}
