/**
 * @license Copyright (c) 2003-2016, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function( config ) {
    this.on("focus", function() {
        // hide all popup's
        jq(".studio-action-panel:not(.freeze-display)").hide();
        jq(".advanced-selector-container").hide();
    });

    config.language = ASC.Resources.Master.TwoLetterISOLanguageName;

    config.enterMode = CKEDITOR.ENTER_P;
    config.shiftEnterMode = CKEDITOR.ENTER_BR;

    CKEDITOR.config.forcePasteAsPlainText = false; // default so content won't be manipulated on load
    CKEDITOR.config.basicEntities = true;
    CKEDITOR.config.entities = true;
    CKEDITOR.config.entities_latin = false;
    CKEDITOR.config.entities_greek = false;
    CKEDITOR.config.entities_processNumerical = false;
    CKEDITOR.config.fillEmptyBlocks = function () {
            return true;
    };
    
    CKEDITOR.config.allowedContent = true; // don't filter my data

    //--------main settings
    config.skin = 'teamlab';
    config.width = '100%';
    config.height = '400px';
    config.resize_dir = 'vertical';
    config.filebrowserWindowWidth = '640px';
    config.filebrowserWindowHeight = '480px';
    config.pasteFromWordRemoveFontStyles = false;
    config.image_previewText = ' ';
    config.disableNativeSpellChecker = false;

    //--------toolbar settings
    this.getBaseConfig = function() {
        return [
            { name: 'basic', items: ['Undo', 'Redo', '-', 'Font', 'FontSize', 'Bold', 'Italic', 'Underline', 'Strike', 'TextColor', 'BGColor'] },
            { name: 'paragraph', items: ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', 'NumberedList', 'BulletedList'] },
            { name: 'insert', items: ['Image', 'Smiley', 'Link', 'Unlink', 'RemoveFormat'] }
        ];
    };

    config.toolbar_Basic =
        config.toolbar_CrmEmail =
            config.toolbar_CrmHistory =
                config.toolbar_ComNews =
                    config.toolbar_ComForum = this.getBaseConfig();

    config.toolbar_Comment = jq.map(this.getBaseConfig(), function (value) {
        if (value.name == "insert") {
            value.items.splice(5, 0, "Blockquote"); // Add 'Blockquote'
            value.items.splice(6, 0, "Source"); // Add 'Source'
        }
        return value;
    });

    config.toolbar_Mail = jq.map(this.getBaseConfig(), function(value) {
        if (value.name == "paragraph") {
            value.items.splice(4, 0, 'Outdent', 'Indent'); // Add 'Outdent', 'Indent'
        }
        if (value.name == "insert") {
            value.items.splice(5, 0, "Source"); // Add 'Source'
            value.items.splice(4, 0, "Blockquote"); // Add 'Blockquote'
        }
        return value;
    });

    config.toolbar_MailSignature = jq.map(this.getBaseConfig(), function(value) {
        if (value.name == "basic") {
            value.items.splice(0, 3); // Remove 'Undo', 'Redo', '-'
        }
        if (value.name == "paragraph") {
            value.items.splice(4, 2); // Remove 'NumberedList', 'BulletedList'
        }
        if (value.name == "insert") {
            value.items.splice(5, 0, "Source"); // Add 'Source'
            value.items.splice(1, 1); // Remove 'Smiley'
        }
        return value;
    });

    config.toolbar_PrjMessage =
        config.toolbar_ComBlog =
            jq.map(this.getBaseConfig(), function(value) {
                if (value.name == "insert") {
                    value.items.splice(5, 0, "Source"); // Add 'Source'
                    value.items.splice(4, 0, 'TeamlabCut'); // Add 'TeamlabCut'
                }
                return value;
            });

    //-------Full toolbar
    config.toolbar_Full =
    [
        { name: 'document', items: ['Source', '-', 'Save', 'NewPage', 'DocProps', 'Preview', 'Print', '-', 'Templates'] },
        { name: 'clipboard', items: ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Undo', 'Redo'] },
        { name: 'editing', items: ['Find', 'Replace', '-', 'SelectAll', '-', 'SpellChecker', 'Scayt'] },
        { name: 'forms', items: ['Form', 'Checkbox', 'Radio', 'TextField', 'Textarea', 'Select', 'Button', 'ImageButton','HiddenField'] },
        '/',
        { name: 'basicstyles', items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
        { name: 'paragraph', items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', 'CreateDiv', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', '-', 'BidiLtr', 'BidiRtl'] },
        { name: 'links', items: ['Link', 'Unlink', 'Anchor'] },
        { name: 'insert', items: ['Image', 'oembed', 'Flash', 'Table', 'HorizontalRule', 'Smiley', 'SpecialChar', 'PageBreak', 'Iframe'] },
        '/',
        { name: 'styles', items: ['Styles', 'Format', 'Font', 'FontSize'] },
        { name: 'colors', items: ['TextColor', 'BGColor'] },
        { name: 'tools', items: ['Maximize', 'ShowBlocks', '-', 'About'] }
    ];

    //--------extraPlugins settings
    config.extraPlugins = 'oembed,teamlabcut,teamlabquote,codemirror';
    config.oembed_maxWidth = '640';
    config.oembed_maxHeight = '480';
    config.oembed_WrapperClass = 'embeded-content';

    //--------teamlabcut settings
    config.teamlabcut_wrapTable = true;

    config.smiley_path = CKEDITOR.basePath + 'plugins/smiley/teamlab_images/',
    config.smiley_images = [
	    'smile1.gif',
        'smile2.gif',
        'smile3.gif',
        'smile4.gif',
        'smile5.gif',
        'smile6.gif',
        'smile7.gif',
        'smile8.gif',
        'smile9.gif',
        'smile10.gif',
        'smile11.gif',
        'smile12.gif',
        'smile13.gif',
        'smile14.gif',
        'smile15.gif',
        'smile16.gif',
        'smile17.gif',
        'smile18.gif',
        'smile19.gif',
        'smile20.gif',
	    'smile21.gif'
    ];
    config.smiley_descriptions = [
        ':-)',
        ';-)',
        ':-\\',
        ':-D',
        ':-(',
        '8-)',
        '*DANCE*',
        '[:-}',
        '*CRAZY*',
        '=-O',
        ':-P',
        ':\'(',
        ':-!',
        '*THUMBS UP*',
        '*SORRY*',
        '*YAHOO*',
        '*OK*',
        ']:->',
        '*HELP*',
        '*DRINK*',
        '@='
    ];
    config.smiley_columns = 5;


    var fonts = config.font_names.split(";");
    fonts.push("Open Sans/Open Sans, sans-serif");
    fonts.sort();
    config.font_names = fonts.join(";");

    config.font_defaultLabel = 'Open Sans';
    config.fontSize_defaultLabel = '12';

};