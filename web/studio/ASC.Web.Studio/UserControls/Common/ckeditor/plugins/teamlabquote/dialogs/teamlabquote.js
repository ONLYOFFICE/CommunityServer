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