/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

/**
* oEmbed Plugin plugin
* Licensed under the MIT license
* jQuery Embed Plugin: http://code.google.com/p/jquery-oembed/ (MIT License)
* Plugin for: http://ckeditor.com/license (GPL/LGPL/MPL: http://ckeditor.com/license)
*/

(function () {
    CKEDITOR.plugins.add('oembed', {
        requires: ['dialog'],
        lang: ['de', 'en', 'fr', 'nl', 'pl', 'ru'],
        afterInit: function (editor) {
            
            var dataProcessor = editor.dataProcessor,
				dataFilter = dataProcessor && dataProcessor.dataFilter;
            
            if (editor.config.oembed_ShowIframePreview) {
                if (dataFilter._.elements.iframe) {
                    delete dataFilter._.elements.iframe;
                }
                return;
			}

            if (dataFilter && dataFilter._ == 'undefined') {
				dataFilter.addRules({
					elements: {
						iframe: function (element) {
							return editor.createFakeParserElement(element, 'cke_iframe', 'iframe', true);
						}
					}
				});
			}
		},
        init: function(editor) {
		    if (editor.config.oembed_ShowIframePreview == null || editor.config.oembed_ShowIframePreview == 'undefined') {
		        editor.config.oembed_ShowIframePreview = false;
		    }

		    if (!editor.plugins.iframe && !editor.config.oembed_ShowIframePreview) {
		        CKEDITOR.addCss('img.cke_iframe' +
		            '{' +
		            'background-image: url(' + CKEDITOR.getUrl(CKEDITOR.plugins.getPath('oembed') + 'images/placeholder.png') + ');' +
		            'background-position: center center;' +
		            'background-repeat: no-repeat;' +
		            'border: 1px solid #a9a9a9;' +
		            'width: 80px;' +
		            'height: 80px;' +
		            '}'
		        );
		    }

		    // Load jquery?
            loadjQueryLibaries();

            CKEDITOR.tools.extend(CKEDITOR.editor.prototype, {
                oEmbed: function (url, maxWidth, maxHeight, responsiveResize) {

                    if (url.length < 1 || url.indexOf('http') < 0) {
                        alert(editor.lang.oembed.invalidUrl);
                        return false;
                    }

                    if (typeof (jQuery.fn.oembed) === 'undefined') {
                        CKEDITOR.scriptLoader.load(CKEDITOR.getUrl(CKEDITOR.plugins.getPath('oembed') + 'libs/jquery.oembed.min.js'), function () {
                            embed();
                        });
                    } else {
                        embed();
                    }
                    
                    function embed() {
                        if (maxWidth == null || maxWidth == 'undefined') {
                            maxWidth = null;
                        }

                        if (maxHeight == null || maxHeight == 'undefined') {
                            maxHeight = null;
                        }

                        if (responsiveResize == null || responsiveResize == 'undefined') {
                            responsiveResize = false;
                        }

                        embedCode(url, editor, false, maxWidth, maxHeight, responsiveResize);
                    }
                    
                    return true;
                }
            });

            editor.addCommand('oembed', new CKEDITOR.dialogCommand('oembed'));
            editor.ui.addButton('oembed', {
                label: editor.lang.oembed.button,
                command: 'oembed',
                icon: this.path + 'images/icon.png'
            });

            var resizeTypeChanged = function () {
                var dialog = this.getDialog(),
                    resizetype = this.getValue(),
                    maxSizeBox = dialog.getContentElement('general', 'maxSizeBox').getElement(),
                    sizeBox = dialog.getContentElement('general', 'sizeBox').getElement();

                if (resizetype == 'noresize') {
                    maxSizeBox.hide();
                    
                    sizeBox.hide();
                } else if (resizetype == "custom") {
                    maxSizeBox.hide();
                    
                    sizeBox.show();
                } else {
                    maxSizeBox.show();
                    
                    sizeBox.hide();
                }

            };
            
            String.prototype.beginsWith = function (string) {
                return (this.indexOf(string) === 0);
            };
            
            function loadjQueryLibaries() {
                if (typeof (jQuery) === 'undefined') {
                    CKEDITOR.scriptLoader.load('http://ajax.googleapis.com/ajax/libs/jquery/1/jquery.min.js', function () {
                        if (typeof (jQuery.fn.oembed) === 'undefined') {
                            CKEDITOR.scriptLoader.load(
                                CKEDITOR.getUrl(CKEDITOR.plugins.getPath('oembed') + 'libs/jquery.oembed.min.js')
                            );
                        }
                    });

                } else if (typeof (jQuery.fn.oembed) === 'undefined') {
                    CKEDITOR.scriptLoader.load(CKEDITOR.getUrl(CKEDITOR.plugins.getPath('oembed') + 'libs/jquery.oembed.min.js'));
                }
            }
            
            function embedCode(url, instance, closeDialog, maxWidth, maxHeight, responsiveResize) {
                jQuery('body').oembed(url, {
                    onEmbed: function (e) {
                        var divWrapper = new CKEDITOR.dom.element('div'),
                            codeElement,
                            codeIframe;
						
						if (typeof e.code === 'string') {
                            if (editor.config.oembed_WrapperClass != null) {
                                divWrapper.addClass(editor.config.oembed_WrapperClass);
                            }

                            codeElement = CKEDITOR.dom.element.createFromHtml(e.code);

                            if (codeElement.$.tagName == "IFRAME" && editor.config.oembed_ShowIframePreview === false) {
								codeIframe = editor.createFakeElement(codeElement, 'cke_iframe', 'iframe', true);
                                codeIframe.appendTo(divWrapper);
                            } else {
                                codeElement.appendTo(divWrapper);
                            }

                            instance.insertElement(divWrapper);

                            if (closeDialog) {
                                CKEDITOR.dialog.getCurrent().hide();
                            }
                        } else if (typeof e.code[0].outerHTML === 'string') {

                            if (editor.config.oembed_WrapperClass != null) {
                                divWrapper.addClass(editor.config.oembed_WrapperClass);
                            }

                            codeElement = CKEDITOR.dom.element.createFromHtml(e.code[0].outerHTML);
							
							if (codeElement.$.tagName == "IFRAME" && editor.config.oembed_ShowIframePreview === false) {
                                codeIframe = editor.createFakeElement(codeElement, 'cke_iframe', 'iframe', true);
                                codeIframe.appendTo(divWrapper);
                            } else {
                                codeElement.appendTo(divWrapper);
                            }

                            instance.insertElement(divWrapper);

                            if (closeDialog) {
                                CKEDITOR.dialog.getCurrent().hide();
                            }
                        } else {
                            alert(editor.lang.oembed.noEmbedCode);
                        }
                    },
                    onError: function (externalUrl) {
                        if (externalUrl.indexOf("vimeo.com") > 0) {
                            alert(editor.lang.oembed.noVimeo);
                        } else {
                            alert(editor.lang.oembed.Error);
                        }
                                
                    },
                    maxHeight: maxHeight,
                    maxWidth: maxWidth,
                    useResponsiveResize: responsiveResize,
                    embedMethod: 'editor'
                });
            }

            CKEDITOR.dialog.add('oembed', function (editor) {
                return {
                    title: editor.lang.oembed.title,
                    minWidth: CKEDITOR.env.ie && CKEDITOR.env.quirks ? 568 : 550,
                    minHeight: 155,
                    onShow: function () {
                        var resizetype = this.getContentElement('general', 'resizeType').getValue(),
                            maxSizeBox = this.getContentElement('general', 'maxSizeBox').getElement(),
                            sizeBox = this.getContentElement('general', 'sizeBox').getElement();

                        if (resizetype == 'noresize') {
                            maxSizeBox.hide();
                            sizeBox.hide();
                        } else if (resizetype == "custom") {
                            maxSizeBox.hide();

                            sizeBox.show();
                        } else {
                            maxSizeBox.show();

                            sizeBox.hide();
                        }
                    },
                    onOk: function () {
                        var inputCode = this.getValueOf('general', 'embedCode'),
                            resizetype = this.getContentElement('general', 'resizeType').
                                getValue(),
                            maxWidth = null,
                            maxHeight = null,
                            responsiveResize = false,
                            editorInstance = this.getParentEditor(),
                            closeDialog = this.getContentElement('general', 'autoCloseDialog').
                                getValue();
                        
                        if (inputCode.length < 1 || inputCode.indexOf('http') < 0) {
                            alert(editor.lang.oembed.invalidUrl);
                            return false;
                        }

                        if (resizetype == "noresize") {
                            responsiveResize = false;
                        } else {
                            if (resizetype == "responsive") {
                                maxWidth = this.getContentElement('general', 'maxWidth').
                                    getInputElement().
                                    getValue();
                                maxHeight = this.getContentElement('general', 'maxHeight').
                                    getInputElement().
                                    getValue();

                                responsiveResize = true;
                            } else if (resizetype == "custom") {
                                maxWidth = this.getContentElement('general', 'width').
                                    getInputElement().
                                    getValue();
                                maxHeight = this.getContentElement('general', 'height').
                                    getInputElement().
                                    getValue();

                                responsiveResize = false;
                            }
                        }

                        // support for multiple urls
                        if (inputCode.indexOf(";") > 0) {
                            var urls = inputCode.split(";");
                            for (var i = 0; i < urls.length; i++) {
                                var url = urls[i];

                                if (url.length > 1 && url.beginsWith('http')) {
                                    embedCode(url, editorInstance, false, maxWidth, maxHeight, responsiveResize);
                                }
                                // close after last
                                if (i == urls.length -1) {
                                    CKEDITOR.dialog.getCurrent().hide();
                                }
                            }
                        } else {
                            // single url
                            embedCode(inputCode, editorInstance, closeDialog, maxWidth, maxHeight, responsiveResize);
                        }
                        
                        return false;
                    },
                    contents: [{
                        label: editor.lang.common.generalTab,
                        id: 'general',
                        elements: [{
                                type: 'html',
                                id: 'oembedHeader',
                                html: '<div style="white-space:normal;width:500px;padding-bottom:10px">' + editor.lang.oembed.pasteUrl + '</div>'
                            }, {
                                type: 'text',
                                id: 'embedCode',
                                focus: function () {
                                    this.getElement().focus();
                                },
                                label: editor.lang.oembed.url,
                                title: editor.lang.oembed.pasteUrl
                            }, {
                                type: 'hbox',
                                widths: ['50%', '50%'],
                                children: [{
                                        id: 'resizeType',
                                        type: 'select',
                                        label: editor.lang.oembed.resizeType,
                                        'default': 'noresize',
                                        items: [
                                            [editor.lang.oembed.noresize, 'noresize'],
                                            [editor.lang.oembed.responsive, 'responsive'],
                                            [editor.lang.oembed.custom, 'custom']
                                        ],
                                        onChange: resizeTypeChanged
                                    }, {
                                        type: 'hbox',
                                        id: 'maxSizeBox',
                                        widths: ['120px', '120px'],
                                        style: 'float:left;position:absolute;left:58%;width:200px',
                                        children: [{
                                            type: 'text',
                                            width:'100px',
                                            id: 'maxWidth',
                                                'default': editor.config.oembed_maxWidth != null ? editor.config.oembed_maxWidth : '560',
                                                label: editor.lang.oembed.maxWidth,
                                                title: editor.lang.oembed.maxWidthTitle
                                            }, {
                                                type: 'text',
                                                id: 'maxHeight',
                                                width: '120px',
                                                'default': editor.config.oembed_maxHeight != null ? editor.config.oembed_maxHeight : '315',
                                                label: editor.lang.oembed.maxHeight,
                                                title: editor.lang.oembed.maxHeightTitle
                                            }]
                                    }, {
                                        type: 'hbox',
                                        id: 'sizeBox',
                                        widths: ['120px', '120px'],
                                        style: 'float:left;position:absolute;left:58%;width:200px',
                                        children: [{
                                                type: 'text',
                                                id: 'width',
                                                width: '100px',
                                                'default': editor.config.oembed_maxWidth != null ? editor.config.oembed_maxWidth : '560',
                                                label: editor.lang.oembed.width,
                                                title: editor.lang.oembed.widthTitle
                                            }, {
                                                type: 'text',
                                                id: 'height',
                                                width: '120px',
                                                'default': editor.config.oembed_maxHeight != null ? editor.config.oembed_maxHeight : '315',
                                                label: editor.lang.oembed.height,
                                                title: editor.lang.oembed.heightTitle
                                            }]
                                    }]
                            }, {
                                type: 'checkbox',
                                id: 'autoCloseDialog',
                                'default': 'checked',
                                label: editor.lang.oembed.autoClose,
                                title: editor.lang.oembed.autoClose
                            }]
                    }]
                };
            });
        }//
    });
    
}

)();