import 'package:flutter/material.dart';

/// A rich markdown text widget tailored for Triple A chat bubbles.
///
/// Automatically parses and renders markdown headers (###, ##, #),
/// bold (**text**), italics (*text*), bullet lists (* or -),
/// numbered lists (1.), code blocks (`code`), and structured tables (| ... |)
/// without showing raw syntax markers.
class FormattedMarkdownText extends StatelessWidget {
  const FormattedMarkdownText({
    super.key,
    required this.text,
    this.style,
    this.headingColor,
    this.boldColor,
    this.bulletColor,
    this.codeBgColor,
    this.dividerColor,
    this.tableHeaderBgColor,
  });

  final String text;
  final TextStyle? style;
  final Color? headingColor;
  final Color? boldColor;
  final Color? bulletColor;
  final Color? codeBgColor;
  final Color? dividerColor;
  final Color? tableHeaderBgColor;

  @override
  Widget build(BuildContext context) {
    final baseStyle = style ?? DefaultTextStyle.of(context).style;
    final blocks = _parseMarkdownBlocks(text, baseStyle);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      mainAxisSize: MainAxisSize.min,
      children: blocks,
    );
  }

  List<Widget> _parseMarkdownBlocks(String rawText, TextStyle baseStyle) {
    final widgets = <Widget>[];
    final lines = rawText.replaceAll('\r\n', '\n').split('\n');

    final paragraphBuffer = <String>[];

    void flushParagraph() {
      if (paragraphBuffer.isEmpty) return;
      final fullParagraph = paragraphBuffer.join(' ').trim();
      paragraphBuffer.clear();
      if (fullParagraph.isNotEmpty) {
        widgets.add(
          Padding(
            padding: const EdgeInsets.only(bottom: 6),
            child: Text.rich(
              TextSpan(
                children: _parseInlineSpans(fullParagraph, baseStyle),
              ),
              style: baseStyle,
            ),
          ),
        );
      }
    }

    final headingRegex = RegExp(r'^(#{1,4})\s+(.*)$');
    final bulletRegex = RegExp(r'^(\*|-|•)\s+(.*)$');
    final numberedRegex = RegExp(r'^(\d+)\.\s+(.*)$');
    final quoteRegex = RegExp(r'^>\s*(.*)$');

    for (int i = 0; i < lines.length; i++) {
      final line = lines[i];
      final trimmed = line.trim();

      // Empty line -> flush pending paragraph & add minor spacing
      if (trimmed.isEmpty) {
        flushParagraph();
        if (widgets.isNotEmpty && i < lines.length - 1) {
          widgets.add(const SizedBox(height: 4));
        }
        continue;
      }

      // Markdown Table block (| ... |)
      if (trimmed.startsWith('|') && trimmed.endsWith('|') && trimmed.length > 2) {
        flushParagraph();
        final tableLines = <String>[trimmed];
        while (i + 1 < lines.length &&
            lines[i + 1].trim().startsWith('|') &&
            lines[i + 1].trim().endsWith('|')) {
          i++;
          tableLines.add(lines[i].trim());
        }

        final tableWidget = _buildStyledTable(tableLines, baseStyle);
        if (tableWidget != null) {
          widgets.add(tableWidget);
        }
        continue;
      }

      // Horizontal divider (---, ***, ___)
      if (trimmed == '---' || trimmed == '***' || trimmed == '___') {
        flushParagraph();
        widgets.add(
          Padding(
            padding: const EdgeInsets.symmetric(vertical: 6),
            child: Divider(
              height: 1,
              color: dividerColor ?? (baseStyle.color?.withValues(alpha: 0.2) ?? Colors.grey.shade300),
            ),
          ),
        );
        continue;
      }

      // Headings (#, ##, ###, ####)
      final headingMatch = headingRegex.firstMatch(trimmed);
      if (headingMatch != null) {
        flushParagraph();
        final level = headingMatch.group(1)!.length;
        final content = headingMatch.group(2)!;

        double sizeBonus = 1.0;
        if (level == 1) {
          sizeBonus = 3.5;
        } else if (level == 2) {
          sizeBonus = 2.5;
        } else if (level == 3) {
          sizeBonus = 1.5;
        }

        final hStyle = baseStyle.copyWith(
          fontSize: (baseStyle.fontSize ?? 13) + sizeBonus,
          fontWeight: FontWeight.bold,
          color: headingColor ?? baseStyle.color,
          height: 1.35,
        );

        widgets.add(
          Padding(
            padding: EdgeInsets.only(
              top: widgets.isEmpty ? 0 : 8,
              bottom: 4,
            ),
            child: Text.rich(
              TextSpan(children: _parseInlineSpans(content, hStyle)),
            ),
          ),
        );
        continue;
      }

      // Blockquote (> ...)
      final quoteMatch = quoteRegex.firstMatch(trimmed);
      if (quoteMatch != null) {
        flushParagraph();
        final content = quoteMatch.group(1)!;
        widgets.add(
          Container(
            margin: const EdgeInsets.symmetric(vertical: 4),
            padding: const EdgeInsets.only(left: 10, top: 4, bottom: 4),
            decoration: BoxDecoration(
              border: Border(
                left: BorderSide(
                  color: bulletColor ?? baseStyle.color?.withValues(alpha: 0.5) ?? Colors.grey,
                  width: 3,
                ),
              ),
            ),
            child: Text.rich(
              TextSpan(
                children: _parseInlineSpans(
                  content,
                  baseStyle.copyWith(fontStyle: FontStyle.italic),
                ),
              ),
            ),
          ),
        );
        continue;
      }

      // Bullet list item (*, -, •)
      final bulletMatch = bulletRegex.firstMatch(trimmed);
      if (bulletMatch != null) {
        flushParagraph();
        final content = bulletMatch.group(2)!;
        widgets.add(
          Padding(
            padding: const EdgeInsets.only(left: 2, top: 2, bottom: 3),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Padding(
                  padding: const EdgeInsets.only(top: 6, right: 8),
                  child: Container(
                    width: 5,
                    height: 5,
                    decoration: BoxDecoration(
                      color: bulletColor ?? baseStyle.color,
                      shape: BoxShape.circle,
                    ),
                  ),
                ),
                Expanded(
                  child: Text.rich(
                    TextSpan(children: _parseInlineSpans(content, baseStyle)),
                    style: baseStyle,
                  ),
                ),
              ],
            ),
          ),
        );
        continue;
      }

      // Numbered list item (1., 2., etc.)
      final numMatch = numberedRegex.firstMatch(trimmed);
      if (numMatch != null) {
        flushParagraph();
        final number = numMatch.group(1)!;
        final content = numMatch.group(2)!;
        widgets.add(
          Padding(
            padding: const EdgeInsets.only(left: 2, top: 2, bottom: 3),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                SizedBox(
                  width: 22,
                  child: Text(
                    '$number.',
                    style: baseStyle.copyWith(
                      fontWeight: FontWeight.bold,
                      color: bulletColor ?? boldColor ?? baseStyle.color,
                    ),
                  ),
                ),
                Expanded(
                  child: Text.rich(
                    TextSpan(children: _parseInlineSpans(content, baseStyle)),
                    style: baseStyle,
                  ),
                ),
              ],
            ),
          ),
        );
        continue;
      }

      // Regular text: collect in buffer to form fluent multi-line paragraphs
      paragraphBuffer.add(trimmed);
    }

    flushParagraph();
    return widgets;
  }

  Widget? _buildStyledTable(List<String> tableLines, TextStyle baseStyle) {
    if (tableLines.length < 2) return null;

    List<String> splitRow(String line) {
      var inner = line.trim();
      if (inner.startsWith('|')) inner = inner.substring(1);
      if (inner.endsWith('|')) inner = inner.substring(0, inner.length - 1);
      return inner.split('|').map((cell) => cell.trim()).toList();
    }

    final headerCells = splitRow(tableLines[0]);
    final columnCount = headerCells.length;
    if (columnCount == 0) return null;

    // Check line 1 is the separator row (contains dashes)
    final sepCells = splitRow(tableLines[1]);
    final isSep = sepCells.isNotEmpty && sepCells.every((c) => RegExp(r'^:?-+:?$').hasMatch(c));
    final dataStartIndex = isSep ? 2 : 1;

    final alignments = <TextAlign>[];
    for (int col = 0; col < columnCount; col++) {
      if (isSep && col < sepCells.length) {
        final sep = sepCells[col];
        if (sep.startsWith(':') && sep.endsWith(':')) {
          alignments.add(TextAlign.center);
        } else if (sep.endsWith(':')) {
          alignments.add(TextAlign.right);
        } else {
          alignments.add(TextAlign.left);
        }
      } else {
        alignments.add(TextAlign.left);
      }
    }

    final headerStyle = baseStyle.copyWith(
      fontSize: (baseStyle.fontSize ?? 13) * 0.92,
      fontWeight: FontWeight.bold,
      color: headingColor ?? baseStyle.color,
    );

    final cellStyle = baseStyle.copyWith(
      fontSize: (baseStyle.fontSize ?? 13) * 0.92,
    );

    final borderColor = dividerColor ?? Colors.black.withValues(alpha: 0.12);
    final headerBg = tableHeaderBgColor ?? (codeBgColor ?? Colors.black.withValues(alpha: 0.07));

    final tableRows = <TableRow>[];

    // Header Row
    tableRows.add(
      TableRow(
        decoration: BoxDecoration(color: headerBg),
        children: List.generate(columnCount, (colIdx) {
          final text = colIdx < headerCells.length ? headerCells[colIdx] : '';
          return Padding(
            padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
            child: Text.rich(
              TextSpan(children: _parseInlineSpans(text, headerStyle)),
              textAlign: alignments[colIdx],
            ),
          );
        }),
      ),
    );

    // Data Rows
    for (int rowIdx = dataStartIndex; rowIdx < tableLines.length; rowIdx++) {
      final cells = splitRow(tableLines[rowIdx]);
      final isEven = (rowIdx - dataStartIndex) % 2 == 0;
      final rowBg = isEven ? Colors.transparent : Colors.black.withValues(alpha: 0.025);

      tableRows.add(
        TableRow(
          decoration: BoxDecoration(color: rowBg),
          children: List.generate(columnCount, (colIdx) {
            final text = colIdx < cells.length ? cells[colIdx] : '';
            return Padding(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 7),
              child: Text.rich(
                TextSpan(children: _parseInlineSpans(text, cellStyle)),
                textAlign: alignments[colIdx],
              ),
            );
          }),
        ),
      );
    }

    return Container(
      margin: const EdgeInsets.symmetric(vertical: 8),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(10),
        border: Border.all(color: borderColor, width: 0.9),
      ),
      clipBehavior: Clip.antiAlias,
      child: SingleChildScrollView(
        scrollDirection: Axis.horizontal,
        physics: const BouncingScrollPhysics(),
        child: Table(
          defaultColumnWidth: const IntrinsicColumnWidth(),
          border: TableBorder(
            horizontalInside: BorderSide(color: borderColor.withValues(alpha: 0.6), width: 0.8),
            verticalInside: BorderSide(color: borderColor.withValues(alpha: 0.4), width: 0.8),
          ),
          children: tableRows,
        ),
      ),
    );
  }

  List<InlineSpan> _parseInlineSpans(String text, TextStyle baseStyle) {
    final spans = <InlineSpan>[];
    final pattern = RegExp(
      r'(\*\*\*([^*]+?)\*\*\*|___([^_]+?)___)'
      r'|(\*\*([^*]+?)\*\*|__([^_]+?)__)'
      r'|(\*([^*]+?)\*|_([^_]+?)_)'
      r'|(`([^`]+?)`)',
    );

    int lastIndex = 0;
    for (final match in pattern.allMatches(text)) {
      if (match.start > lastIndex) {
        spans.add(TextSpan(
          text: text.substring(lastIndex, match.start),
          style: baseStyle,
        ));
      }

      final fullMatch = match.group(0)!;
      if (fullMatch.startsWith('***') || fullMatch.startsWith('___')) {
        final content = match.group(2) ?? match.group(3) ?? '';
        spans.add(TextSpan(
          text: content,
          style: baseStyle.copyWith(
            fontWeight: FontWeight.bold,
            fontStyle: FontStyle.italic,
            color: boldColor ?? baseStyle.color,
          ),
        ));
      } else if (fullMatch.startsWith('**') || fullMatch.startsWith('__')) {
        final content = match.group(5) ?? match.group(6) ?? '';
        spans.add(TextSpan(
          text: content,
          style: baseStyle.copyWith(
            fontWeight: FontWeight.bold,
            color: boldColor ?? baseStyle.color,
          ),
        ));
      } else if (fullMatch.startsWith('*') || fullMatch.startsWith('_')) {
        final content = match.group(8) ?? match.group(9) ?? '';
        spans.add(TextSpan(
          text: content,
          style: baseStyle.copyWith(
            fontStyle: FontStyle.italic,
          ),
        ));
      } else if (fullMatch.startsWith('`')) {
        final content = match.group(11) ?? '';
        spans.add(WidgetSpan(
          alignment: PlaceholderAlignment.middle,
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 5, vertical: 1),
            decoration: BoxDecoration(
              color: codeBgColor ?? baseStyle.color?.withValues(alpha: 0.1) ?? Colors.black12,
              borderRadius: BorderRadius.circular(4),
            ),
            child: Text(
              content,
              style: baseStyle.copyWith(
                fontFamily: 'monospace',
                fontSize: (baseStyle.fontSize ?? 13) * 0.9,
              ),
            ),
          ),
        ));
      }

      lastIndex = match.end;
    }

    if (lastIndex < text.length) {
      spans.add(TextSpan(
        text: text.substring(lastIndex),
        style: baseStyle,
      ));
    }

    return spans;
  }
}
