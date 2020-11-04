CKEDITOR.dialog.add('teamlabquoteDialog', function (editor) {
    var lang = editor.lang.teamlabquote;

    return {

        title: lang.dialogTitle,
		minWidth: 400,
		minHeight: 200,

		contents: [
			{
				id: 'tab-basic',
				label: lang.dialogTitle,

				elements: [
					{
						type: 'text',
						id: 'name',
						label: editor.lang.common.name + ':',
						validate: CKEDITOR.dialog.validate.notEmpty(lang.errorMessage)
					},
					{
					    type: 'textarea',
						id: 'qoute',
						label: lang.quote + ':'
					}
				]
			}
		],

		onOk: function() {
			var dialog = this;

			var html = '<div class="mainQuote">';
			html += '	<div class="quoteCaption"><span class="bold">' + dialog.getValueOf( 'tab-basic', 'name' ) + '</span> ' + lang.wrote + ':</div>';
			html += '		<div id="quote">';
			html += '		<div class="bord"><div class="t"><div class="r">';
			html += '		<div class="b"><div class="l"><div class="c">';
			html += '			<div class="reducer">'+ dialog.getValueOf('tab-basic', 'qoute') +'</div>';
			html += '		</div></div></div>';
			html += '		</div></div></div>';
			html += '	</div>';
			html += '</div>';

			var tabElement = CKEDITOR.dom.element.createFromHtml(html, editor.document);
			editor.insertElement(tabElement);
		}
	};
});