CKEDITOR.plugins.add( 'teamlabquote', {

    lang: 'de,en,es,fr,it,ru',
    icons: 'teamlabquote',
	init: function( editor ) {

	    var lang = editor.lang.teamlabquote;

	    editor.addCommand('teamlabquote', new CKEDITOR.dialogCommand('teamlabquoteDialog'));

	    editor.ui.addButton('TeamlabQuote', {

	        label: lang.addButtonTitle,
			command: 'teamlabquote',
			toolbar: 'insert'
		});

	    CKEDITOR.dialog.add('teamlabquoteDialog', this.path + 'dialogs/teamlabquote.js');
	}
});

