/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


function reviewBuilder(reviewArray, reviewLocaleArray, templateID) {
    if(typeof reviewLocaleArray != "undefined") {
        reviewArray = _.merge(reviewArray,reviewLocaleArray);    
    }
    var listHtml = "",
        templateHtml = jq(templateID).html(),
        reviewHeading = "",
        authorChannel = "";
    for(var key in reviewArray.reviews){
        var ratingScore = "";
        if(typeof reviewArray.reviews[key]["rating"] != "undefined"){
            for(var i=0; i<reviewArray.reviews[key]["rating"]; i++){
                ratingScore += '<div class="rev-stat star_colored"></div>';
            }
            for(var i=0; i<(5 - reviewArray.reviews[key]["rating"]); i++){
                ratingScore += '<div class="rev-stat star_empty"></div>';
            }
        }
        if (typeof reviewArray.reviews[key]["author-name"] != "undefined" && reviewArray.reviews[key]["author-name"] != "") {
            reviewHeading = '<div class="review_block_heading">' + reviewArray.reviews[key]["review-heading"] + '</div>';
        } else {
            reviewHeading = "";
        }
        if(typeof reviewArray.reviews[key]["review-heading"] != "undefined") {
            reviewHeading = '<div class="review_block_heading">' + reviewArray.reviews[key]["review-heading"] + '</div>';
        } else {
            reviewHeading = "";
        }
        listHtml += templateHtml.replace(/{{data-order}}/g,         key)
                                .replace(/{{author-name}}/g,        reviewArray.reviews[key]["author-name"])
                                .replace(/{{review-text}}/g,        reviewArray.reviews[key]["review-text"])
                                .replace(/{{review-heading}}/g,     reviewHeading)
                                .replace(/{{rating}}/g,             ratingScore);
    }
    
    jq(".heartheweb_reviews_list").html(listHtml);
    
    var e = [];

    for(var i=0; i<order.length; i++) {
        jq('.review_block').each(function() {
            var t = jq(this).attr('data-order');
            if(order[i] == t) {
                e.push(jq(this));
            }
        })
    }

    jq(".heartheweb_reviews_list").children(".review_block").remove();
    jq(".heartheweb_reviews_list").append(e).fadeIn();
   
    jq('.heartheweb_reviews_list').masonry({
        itemSelector: '.review_block',
        columnWidth: '.review_block',
        gutter: 16
    });

    var revBlCleanUp;
    updateReviewsBlock();
    jq(window).on("resize", function () {
        if (revBlCleanUp) {
            jq(".review_block").css({ "width": "", "margin-left": "" });
            revBlCleanUp = false;
        } else {
            revBlCleanUp = true;
        }
    });

    function updateReviewsBlock() {
        var lefts = {};
        var midLeft = maxLeft = 0;
        jq(".review_block").each(function () {
            var dataOrder = jq(this).attr("data-order");
            var rbLeft = parseInt(jq(this).css("left"));
            if (rbLeft > midLeft) {
                if (rbLeft >= maxLeft) {
                    maxLeft = rbLeft;
                } else {
                    midLeft = rbLeft;
                }
            }
            if (lefts[rbLeft] != undefined) {
                lefts[rbLeft].push(dataOrder);
            } else {
                lefts[rbLeft] = [dataOrder];
            }
        });

        var windowWidth = jQuery(window).width();
        if (windowWidth > 1200) {
            for (var i = 0; i < lefts[midLeft].length; i++) {
                jq(".review_block[data-order=" + lefts[midLeft][i] + "]").css("width", "368px");
            }
            for (var i = 0; i < lefts[maxLeft].length; i++) {
                jq(".review_block[data-order=" + lefts[maxLeft][i] + "]").css("margin-left", "8px");
            }
        } else if (windowWidth >= 1024 && windowWidth <= 1200) {
            for (var i = 0; i < lefts[midLeft].length; i++) {
                jq(".review_block[data-order=" + lefts[midLeft][i] + "]").css("width", "312px");
            }
            for (var i = 0; i < lefts[maxLeft].length; i++) {
                jq(".review_block[data-order=" + lefts[maxLeft][i] + "]").css("margin-left", "23px").css("width", "295px");
            }
        } else {
            jq(".review_block").css({ "width": "", "margin-left": "" });
        }
        revBlCleanUp = false;
    };
}