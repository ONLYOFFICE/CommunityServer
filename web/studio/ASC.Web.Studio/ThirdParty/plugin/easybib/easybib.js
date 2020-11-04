(function (window, undefined) {

    
    var foundData;

    var fileId = null,
        bibliographyStyle = null,
        bibliographyStorage = null;

    var BibList;
    $(document).ready(function () {
        $("#styles").select2();
    });
    window.Asc.plugin.init = function () {
        
        BibList = new Bibliography();
        var docBody = frames.parent.parent.document.getElementsByTagName("body");
        var searchInput = $('#search');

        window.Asc.plugin.onTranslate = applyTranslations;

        searchInput.attr('placeholder', getMessage("Enter book title"));

        if (docBody) {
            var action = $(docBody).find('form').attr('action');
            if (action && typeof action == 'string') {
                if (action.split('fileid=').length > 0) {
                    fileId = action.split('fileid=')[1];
                }
            }
        }
        if (localStorageManager.isAvailable) {
            bibliographyStorage = localStorageManager.getItem(BibList.localStorageKey);
        }
        
        if (bibliographyStorage && fileId) {
            BibList.bibliography = bibliographyStorage[fileId] ? bibliographyStorage[fileId] : [];
            if (BibList.bibliography && BibList.bibliography.length > 0) {
                $("#search_result").empty();
               
                $("#search_result").hide();
                $(".result-container .search-title").hide();
                $(".result-container .bibliography-title").show();
                $("#bib").show();
                
                bibliographyStyle = BibList.bibliography[0].data.style;
                
                for (var i = BibList.bibliography.length - 1; i >= 0; i--) {
                    BibList.createCitations(BibList.bibliography[i].id, BibList.bibliography[i].data);
                }
            };
        }
        
        $.ajax({
            url: '/api/2.0/files/easybib-styles',
            type: "GET",
            success: function (res) {
                if (res.response.success) {
                    try {
                        var styles = JSON.parse(res.response.styles);
                        BibList.fillListStyles(styles.data, bibliographyStyle);
                    } catch(e) {
                        console.log(e.message);
                    } 
                }
            },
            error: function(err) {
                console.log(err);
            }
        });
        $('.source-button').on('click', function () {
            
            $('#main-container .buttons-container .active-button').removeClass('active-button');
            $(this).addClass('active-button');
            $('#search')[0].value = "";
            $('#search').focus();
            switch (this.id) {
                case 'bookButton':
                    BibList.searchOptions.source = 0;
                    searchInput.attr('placeholder', getMessage("Enter book title"));
                    $('#journalButton').removeClass('web-active-button');
                    $('#journalButton').addClass('book-active-button');
                    break;
                case 'journalButton':
                    BibList.searchOptions.source = 1;
                    $('#journalButton').removeClass('web-active-button');
                    $('#journalButton').removeClass('book-active-button');
                    searchInput.attr('placeholder', getMessage("Enter journal article"));
                    break;
                case 'websiteButton':
                    BibList.searchOptions.source = 2;
                    $('#journalButton').removeClass('book-active-button');
                    $('#journalButton').addClass('web-active-button');
                    searchInput.attr('placeholder', getMessage("Enter website url"));
                    break;
                default:
                    break;
            }
        });
        $('#searchButton').on('click', function () {
            if ($('#search')[0].value) {
                $.ajax({
                    url: '/api/2.0/files/easybib-citation-list',
                    type: "GET",
                    data: {
                        source: BibList.searchOptions.source,
                        data: searchInput[0].value
                    },
                    success: function (res) {
                        if (res.response.success && res.response.citations != "error") {
                            try {
                                var data = JSON.parse(res.response.citations);
                                foundData = data;
                                $("#search_result").empty();
                                switch (BibList.searchOptions.source) {
                                    case 0:
                                        BibList.showBookSearchResult(data.results);
                                        break;
                                    case 1:
                                        BibList.showJournalSearchResult(data.results);
                                        break;
                                    case 2:
                                        BibList.showWebSiteSearchResult(data.results);
                                        break;
                                    default:
                                }
                                res = null;
                            } catch(e) {

                            } 
                        }
                    },
                });
            }
        });
        var entityMap = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&quot;',
            "/": '&#x2F;'
        };

        function escapeHtml(string) {
            return String(string).replace(/[&<>"'`=\/]/g, function (s) {
                return entityMap[s];
            });
        }
        $('.add-button').on('click', function () {
            if (BibList.bibliography.length > 0) {
                var plugin = window.Asc.plugin;

                plugin.info.recalculate = true;
                var sScript = 'var oDocument = Api.GetDocument();';

                sScript += 'var oParagraph = Api.CreateParagraph();';
                sScript += 'oParagraph.SetTabs([4320, 7200], ["center", "right"]);';
                sScript += 'oParagraph.AddTabStop();';
                sScript += 'oParagraph.AddText(\'Bibliography\');';
                sScript += 'oParagraph.AddLineBreak();';

                BibList.bibliographyText.forEach(function (item) {
                    var tmp = $("<div>" + escapeHtml(item.data) + "</div>");
                    if (tmp[0].childNodes.length > 0) {
                        tmp[0].childNodes.forEach(function (itemChild) {
                            switch (itemChild.nodeName) {
                                case "#text":
                                    sScript += 'oRun = Api.CreateRun();';
                                    sScript += 'oRun.AddText(\'' + itemChild.textContent + '\');';
                                    sScript += 'oParagraph.AddElement(oRun);';
                                    break;
                                case "B":
                                    sScript += 'oRun = Api.CreateRun();';
                                    sScript += 'oRun.SetBold(true);';
                                    sScript += 'oRun.AddText(\'' + itemChild.textContent + '\');';
                                    sScript += 'oParagraph.AddElement(oRun);';

                                    break;
                                case "I":
                                    sScript += 'oRun = Api.CreateRun();';
                                    sScript += 'oRun.SetItalic(true);';
                                    sScript += 'oRun.AddText(\'' + itemChild.textContent + '\');';
                                    sScript += 'oParagraph.AddElement(oRun);';
                                    break;
                                default:
                            }
                        });
                    }
                    sScript += 'oParagraph.AddLineBreak();';
                });
                sScript += 'oDocument.InsertContent([oParagraph]);';
                plugin.executeCommand("command", sScript);
            }
            
        });
        $('#search_result').on('scroll', function () {
            var searchResult = $('#search_result');
            if (this.scrollTop == 0) {
                searchResult.removeClass('top-border');
            } else {
                searchResult.addClass('top-border');
            }
            if (this.scrollTop == this.scrollHeight - this.clientHeight) {
                searchResult.removeClass('bottom-border');
            } else {
                searchResult.addClass('bottom-border');
            }
        });
        $('#bib').on('scroll', function () {
            var searchResult = $('#bib');
            if (this.scrollTop == 0) {
                searchResult.removeClass('top-border');
            } else {
                searchResult.addClass('top-border');
            }
            if (this.scrollTop == this.scrollHeight - this.clientHeight) {
                searchResult.removeClass('bottom-border');
            } else {
                searchResult.addClass('bottom-border');
            }
        });
        $("#styles").change(function () {
            var newStyle = $(this).val();
            $('#bib').empty();
            if (BibList.bibliography.length > 0) {
                for (var i = BibList.bibliography.length - 1; i >= 0; i--) {
                    BibList.bibliography[i].data.style = newStyle;
                    BibList.updateCitations(BibList.bibliography[i].id, BibList.bibliography[i].data);
                }
                bibliographyStorage[fileId] = BibList.bibliography;
                localStorageManager.setItem(BibList.localStorageKey, bibliographyStorage);
            };
        }); 
    };

    function applyTranslations() {
        var elements = document.getElementsByClassName("i18n");

        for (var i = 0; i < elements.length; i++) {
            var el = elements[i];
            if (el.attributes["placeholder"]) el.attributes["placeholder"].value = getMessage(el.attributes["placeholder"].value);
            if (el.innerText) el.innerText = getMessage(el.innerText);
        }
    }

    function getMessage(key) {
        return window.Asc.plugin.tr(key);
    }

    delBibliographyPart = function (data) {
        var id = +$(data)[0].id.split('_')[1];
        if (id && id > 0) {
            if (BibList.bibliography.length > 0) {
                BibList.bibliography.forEach(function (bibliographyItem, i) {
                    if (bibliographyItem.id == id) {
                        BibList.bibliography.splice(i, 1);
                    }
                });
                BibList.bibliographyText.forEach(function (bibliographyTextItem, i) {
                    if (bibliographyTextItem.id == id) {
                        BibList.bibliographyText.splice(i, 1);
                    }
                });
            }
            $('#bib').empty();
            if (BibList.bibliographyText.length > 0) {
                BibList.bibliographyText.forEach(function (item_bibliographyText) {
                    createBibItem(item_bibliographyText.id, item_bibliographyText.data);
                });
            }
            bibliographyStorage[fileId] = BibList.bibliography;
            localStorageManager.setItem(BibList.localStorageKey, bibliographyStorage);
        }
    };
    addItem = function(data) {
        var id = $(data)[0].parentNode.parentNode.id;
        BibList.getCitation(id, foundData,bibliographyStorage, fileId);
    };

    backToBibliography = function() {
        $("#search_result").empty();
        
        $("#search_result").hide();
        $(".result-container .search-title").hide();
        $(".result-container .bibliography-title").show();
        $("#bib").show();
    };
    window.Asc.plugin.onMethodReturn = function () {
    };
    window.Asc.plugin.button = function () {
        this.executeCommand("close", '');
    };

   
})(window);