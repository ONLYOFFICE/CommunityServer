<%@ Page Language="C#" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en">
    <head>
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <link rel="icon" href="/TenantLogo.ashx?logotype=3" type="image/x-icon" />
        <link href="https://fonts.googleapis.com/css?family=Open+Sans:800,700,600,500,400,300&subset=latin,cyrillic-ext,cyrillic,latin-ext" rel="stylesheet" type="text/css" />
        <title>Error 403. Access forbidden</title>

        <style type="text/css">
            html {
                height: 100%;
                font-family: Open Sans;
                font-style: normal;
                font-weight: normal;
                color: #333333;
                margin: 0;
                padding: 0;
                border: 0;
                box-sizing: border-box;
                overflow-x: hidden;
            }

            body {
                background: #FFFFFF;
                cursor: default;
                height: 100%;
                min-height: 100%;
                overflow-x: hidden;
                display: flex;
                flex-direction: column;
                align-items: center;
                margin: 0;
                padding: 36px 36px 0 36px;
                border: 0;
                box-sizing: border-box;
            }

            #logo-container {
                margin: 0 0 auto 0;
                height: 35px;
                max-height: 35px;
            }

            #logo {
                height: 35px;
                width: 216px;
            }

            #container {
                position: relative;
                margin: 12px 0 60px 0;
                max-width: 90vh;
            }

            #header {
                font-weight: 600;
                font-size: 30px;
                line-height: 41px;
                margin: 0 0 24px 0;
                text-align: center;
            }

            #text {
                font-size: 16px;
                line-height: 22px;
                text-align: center;
                margin: 0 0 24px 0;
                max-width: 560px;
                padding: 0;
            }

            #button-container {
                margin: 0 0 auto 0;
            }

            #button {
                display: inline-block;
                background: #33B2E3;
                border-radius: 3px;
                color: #FFFFFF;
                padding: 12px 20px;
                font-weight: 600;
                font-size: 14px;
                line-height: 19px;
                text-decoration: none;
                text-align: center;
                margin: 0 0 36px 0;
                box-sizing: border-box;
            }

            @media screen and (max-width: 960px) {
                body {
                    padding: 24px 24px 0 24px;
                }

                #container {
                    margin: 12px 0 48px 0;
                }

                #button {
                    margin: 0 0 24px 0;
                }
            }

            @media screen and (max-width: 620px) {
                body {
                    padding: 18px 18px 0 18px;
                }

                #container {
                    margin: 12px 0 36px 0;
                }

                #header {
                    font-size: 18px;
                    line-height: 25px;
                }

                #text {
                    font-size: 13px;
                    line-height: 18px;
                }

                #button-container {
                    align-self: stretch;
                    margin: auto 0 0 0;
                }

                #button {
                    width: 100%;
                    margin: 0 0 18px 0;
                }
            }
        </style>

        <style type="text/css">
            #background {
                width: 100%;
                height: auto;
                -webkit-animation: fadein_background 1s;
                -moz-animation: fadein_background 1s;
                -ms-animation: fadein_background 1s;
                -o-animation: fadein_background 1s;
                animation: fadein_background 1s;
            }

            @keyframes fadein_background {
                from {
                    opacity: 0;
                }

                to {
                    opacity: 1;
                }
            }

            @-moz-keyframes fadein_background {
                from {
                    opacity: 0;
                }

                to {
                    opacity: 1;
                }
            }

            @-webkit-keyframes fadein_background {
                from {
                    opacity: 0;
                }

                to {
                    opacity: 1;
                }
            }

            @-ms-keyframes fadein_background {
                from {
                    opacity: 0;
                }

                to {
                    opacity: 1;
                }
            }

            #birds {
                position: absolute;
                left: 56.8%;
                top: 27.4%;
                width: 35%;
                height: 33.7%;
                z-index: 100;
                -webkit-animation: fadein_birds 1s;
                -moz-animation: fadein_birds 1s;
                -ms-animation: fadein_birds 1s;
                -o-animation: fadein_birds 1s;
                animation: fadein_birds 1s;
            }

            @keyframes fadein_birds {
                from {
                    opacity: 0;
                    left: 56.8%;
                    top: 0;
                }

                to {
                    opacity: 1;
                    left: 56.8%;
                    top: 27.4%;
                }
            }

            @-moz-keyframes fadein_birds {
                from {
                    opacity: 0;
                    left: 56.8%;
                    top: 0;
                }

                to {
                    opacity: 1;
                    left: 56.8%;
                    top: 27.4%;
                }
            }

            @-webkit-keyframes fadein_birds {
                from {
                    opacity: 0;
                    left: 56.8%;
                    top: 0;
                }

                to {
                    opacity: 1;
                    left: 56.8%;
                    top: 27.4%;
                }
            }

            @-ms-keyframes fadein_birds {
                from {
                    opacity: 0;
                    left: 56.8%;
                    top: 0;
                }

                to {
                    opacity: 1;
                    left: 56.8%;
                    top: 27.4%;
                }
            }

            #mountain-left {
                position: absolute;
                left: 10.66%;
                top: 63.01%;
                width: 25.46%;
                height: 35.61%;
                z-index: 200;
                -webkit-animation: fadein_mountain-left 1s;
                -moz-animation: fadein_mountain-left 1s;
                -ms-animation: fadein_mountain-left 1s;
                -o-animation: fadein_mountain-left 1s;
                animation: fadein_mountain-left 1s;
            }

            @keyframes fadein_mountain-left {
                from {
                    opacity: 0;
                    left: 10.66%;
                    top: 90.40%;
                }

                to {
                    opacity: 1;
                    left: 10.66%;
                    top: 63.01%;
                }
            }

            @-moz-keyframes fadein_mountain-left {
                from {
                    opacity: 0;
                    left: 10.66%;
                    top: 90.40%;
                }

                to {
                    opacity: 1;
                    left: 10.66%;
                    top: 63.01%;
                }
            }

            @-webkit-keyframes fadein_mountain-left {
                from {
                    opacity: 0;
                    left: 10.66%;
                    top: 90.40%;
                }

                to {
                    opacity: 1;
                    left: 10.66%;
                    top: 63.01%;
                }
            }

            @-ms-keyframes fadein_mountain-left {
                from {
                    opacity: 0;
                    left: 10.66%;
                    top: 90.40%;
                }

                to {
                    opacity: 1;
                    left: 10.66%;
                    top: 63.01%;
                }
            }

            #mountain-right {
                position: absolute;
                left: 58.66%;
                top: 54.79%;
                width: 30.66%;
                height: 44.38%;
                z-index: 300;
                -webkit-animation: fadein_mountain-right 1s;
                -moz-animation: fadein_mountain-right 1s;
                -ms-animation: fadein_mountain-right 1s;
                -o-animation: fadein_mountain-right 1s;
                animation: fadein_mountain-right 1s;
            }

            @keyframes fadein_mountain-right {
                from {
                    opacity: 0;
                    left: 58.66%;
                    top: 82.19%;
                }

                to {
                    opacity: 1;
                    left: 58.66%;
                    top: 54.79%;
                }
            }

            @-moz-keyframes fadein_mountain-right {
                from {
                    opacity: 0;
                    left: 58.66%;
                    top: 82.19%;
                }

                to {
                    opacity: 1;
                    left: 58.66%;
                    top: 54.79%;
                }
            }

            @-webkit-keyframes fadein_mountain-right {
                from {
                    opacity: 0;
                    left: 58.66%;
                    top: 82.19%;
                }

                to {
                    opacity: 1;
                    left: 58.66%;
                    top: 54.79%;
                }
            }

            @-ms-keyframes fadein_mountain-right {
                from {
                    opacity: 0;
                    left: 58.66%;
                    top: 82.19%;
                }

                to {
                    opacity: 1;
                    left: 58.66%;
                    top: 54.79%;
                }
            }

            #mountain-center {
                position: absolute;
                left: 24.8%;
                top: 45.47%;
                width: 48.53%;
                height: 66.30%;
                z-index: 400;
                -webkit-animation: fadein_mountain-center 1s;
                -moz-animation: fadein_mountain-center 1s;
                -ms-animation: fadein_mountain-center 1s;
                -o-animation: fadein_mountain-center 1s;
                animation: fadein_mountain-center 1s;
            }

            @keyframes fadein_mountain-center {
                from {
                    opacity: 0;
                    left: 24.8%;
                    top: 72.87%;
                }

                to {
                    opacity: 1;
                    left: 24.8%;
                    top: 45.47%;
                }
            }

            @-moz-keyframes fadein_mountain-center {
                from {
                    opacity: 0;
                    left: 24.8%;
                    top: 72.87%;
                }

                to {
                    opacity: 1;
                    left: 24.8%;
                    top: 45.47%;
                }
            }

            @-webkit-keyframes fadein_mountain-center {
                from {
                    opacity: 0;
                    left: 24.8%;
                    top: 72.87%;
                }

                to {
                    opacity: 1;
                    left: 24.8%;
                    top: 45.47%;
                }
            }

            @-ms-keyframes fadein_mountain-center {
                from {
                    opacity: 0;
                    left: 24.8%;
                    top: 72.87%;
                }

                to {
                    opacity: 1;
                    left: 24.8%;
                    top: 45.47%;
                }
            }

            #white-cloud-behind {
                position: absolute;
                left: 57.33%;
                top: 63.01%;
                width: 8.4%;
                height: 7.39%;
                z-index: 350;
                -webkit-animation: fadein_white-cloud-behind 1s ease-in, move_white-cloud-behind 1s linear 1s infinite alternate;
                -moz-animation: fadein_white-cloud-behind 1s ease-in, move_white-cloud-behind 1s linear 1s infinite alternate;
                -ms-animation: fadein_white-cloud-behind 1s ease-in, move_white-cloud-behind 1s linear 1s infinite alternate;
                -o-animation: fadein_white-cloud-behind 1s ease-in, move_white-cloud-behind 1s linear 1s infinite alternate;
                animation: fadein_white-cloud-behind 1s ease-in, move_white-cloud-behind 1s linear 1s infinite alternate;
            }

            @keyframes fadein_white-cloud-behind {
                from {
                    opacity: 0;
                    left: 57.33%;
                    top: 90.41%;
                }

                to {
                    opacity: 1;
                    left: 57.33%;
                    top: 63.01%;
                }
            }

            @-moz-keyframes fadein_white-cloud-behind {
                from {
                    opacity: 0;
                    left: 57.33%;
                    top: 90.41%;
                }

                to {
                    opacity: 1;
                    left: 57.33%;
                    top: 63.01%;
                }
            }

            @-webkit-keyframes fadein_white-cloud-behind {
                from {
                    opacity: 0;
                    left: 57.33%;
                    top: 90.41%;
                }

                to {
                    opacity: 1;
                    left: 57.33%;
                    top: 63.01%;
                }
            }

            @-ms-keyframes fadein_white-cloud-behind {
                from {
                    opacity: 0;
                    left: 57.33%;
                    top: 90.41%;
                }

                to {
                    opacity: 1;
                    left: 57.33%;
                    top: 63.01%;
                }
            }

            @keyframes move_white-cloud-behind {
                from {
                    top: 63.01%;
                }

                to {
                    top: 64.65%;
                }
            }

            @-moz-keyframes move_white-cloud-behind {
                from {
                    top: 63.01%;
                }

                to {
                    top: 64.65%;
                }
            }

            @-webkit-keyframes move_white-cloud-behind {
                from {
                    top: 63.01%;
                }

                to {
                    top: 64.65%;
                }
            }

            @-ms-keyframes move_white-cloud-behind {
                from {
                    top: 63.01%;
                }

                to {
                    top: 64.65%;
                }
            }

            #white-cloud-center {
                position: absolute;
                left: 31.33%;
                top: 73.97%;
                width: 9.86%;
                height: 9.04%;
                z-index: 600;
                -webkit-animation: fadein_white-cloud-center 1s ease-in, move_white-cloud-center 1s linear 1s infinite alternate;
                -moz-animation: fadein_white-cloud-center 1s ease-in, move_white-cloud-center 1s linear 1s infinite alternate;
                -ms-animation: fadein_white-cloud-center 1s ease-in, move_white-cloud-center 1s linear 1s infinite alternate;
                -o-animation: fadein_white-cloud-center 1s ease-in, move_white-cloud-center 1s linear 1s infinite alternate;
                animation: fadein_white-cloud-center 1s ease-in, move_white-cloud-center 1s linear 1s infinite alternate;
            }

            @keyframes fadein_white-cloud-center {
                from {
                    opacity: 0;
                    left: 31.33%;
                    top: 101.36%;
                }

                to {
                    opacity: 1;
                    left: 31.33%;
                    top: 73.97%;
                }
            }

            @-moz-keyframes fadein_white-cloud-center {
                from {
                    opacity: 0;
                    left: 31.33%;
                    top: 101.36%;
                }

                to {
                    opacity: 1;
                    left: 31.33%;
                    top: 73.97%;
                }
            }

            @-webkit-keyframes fadein_white-cloud-center {
                from {
                    opacity: 0;
                    left: 31.33%;
                    top: 101.36%;
                }

                to {
                    opacity: 1;
                    left: 31.33%;
                    top: 73.97%;
                }
            }

            @-ms-keyframes fadein_white-cloud-center {
                from {
                    opacity: 0;
                    left: 31.33%;
                    top: 101.36%;
                }

                to {
                    opacity: 1;
                    left: 31.33%;
                    top: 73.97%;
                }
            }

            @keyframes move_white-cloud-center {
                from {
                    top: 73.97%;
                }

                to {
                    top: 72.32%;
                }
            }

            @-moz-keyframes move_white-cloud-center {
                from {
                    top: 73.97%;
                }

                to {
                    top: 72.32%;
                }
            }

            @-webkit-keyframes move_white-cloud-center {
                from {
                    top: 73.97%;
                }

                to {
                    top: 72.32%;
                }
            }

            @-ms-keyframes move_white-cloud-center {
                from {
                    top: 73.97%;
                }

                to {
                    top: 72.32%;
                }
            }

            #white-cloud-left {
                position: absolute;
                left: -0.66%;
                top: 80.82%;
                width: 24%;
                height: 21.91%;
                z-index: 700;
                -webkit-animation: fadein_white-cloud-left 1s ease-in;
                -moz-animation: fadein_white-cloud-left 1s ease-in;
                -ms-animation: fadein_white-cloud-left 1s ease-in;
                -o-animation: fadein_white-cloud-left 1s ease-in;
                animation: fadein_white-cloud-left 1s ease-in;
            }

            @keyframes fadein_white-cloud-left {
                from {
                    opacity: 0;
                    left: -14%;
                    top: 80.82%;
                }

                to {
                    opacity: 1;
                    left: -0.66%;
                    top: 80.82%;
                }
            }

            @-moz-keyframes fadein_white-cloud-left {
                from {
                    opacity: 0;
                    left: -14%;
                    top: 80.82%;
                }

                to {
                    opacity: 1;
                    left: -0.66%;
                    top: 80.82%;
                }
            }

            @-webkit-keyframes fadein_white-cloud-left {
                from {
                    opacity: 0;
                    left: -14%;
                    top: 80.82%;
                }

                to {
                    opacity: 1;
                    left: -0.66%;
                    top: 80.82%;
                }
            }

            @-ms-keyframes fadein_white-cloud-left {
                from {
                    opacity: 0;
                    left: -14%;
                    top: 80.82%;
                }

                to {
                    opacity: 1;
                    left: -0.66%;
                    top: 80.82%;
                }
            }

            #white-cloud-right {
                position: absolute;
                left: 81.33%;
                top: 86.30%;
                width: 21.33%;
                height: 19.17%;
                z-index: 800;
                -webkit-animation: fadein_white-cloud-right 1s ease-in;
                -moz-animation: fadein_white-cloud-right 1s ease-in;
                -ms-animation: fadein_white-cloud-right 1s ease-in;
                -o-animation: fadein_white-cloud-right 1s ease-in;
                animation: fadein_white-cloud-right 1s ease-in;
            }

            @keyframes fadein_white-cloud-right {
                from {
                    opacity: 0;
                    left: 94.66%;
                    top: 86.30%;
                }

                to {
                    opacity: 1;
                    left: 81.33%;
                    top: 86.30%;
                }
            }

            @-moz-keyframes fadein_white-cloud-right {
                from {
                    opacity: 0;
                    left: 94.66%;
                    top: 86.30%;
                }

                to {
                    opacity: 1;
                    left: 81.33%;
                    top: 86.30%;
                }
            }

            @-webkit-keyframes fadein_white-cloud-right {
                from {
                    opacity: 0;
                    left: 94.66%;
                    top: 86.30%;
                }

                to {
                    opacity: 1;
                    left: 81.33%;
                    top: 86.30%;
                }
            }

            @-ms-keyframes fadein_white-cloud-right {
                from {
                    opacity: 0;
                    left: 94.66%;
                    top: 86.30%;
                }

                to {
                    opacity: 1;
                    left: 81.33%;
                    top: 86.30%;
                }
            }

            #blue-cloud-left {
                position: absolute;
                left: 0;
                top: 43.83%;
                width: 8.4%;
                height: 6.57%;
                z-index: 900;
                -webkit-animation: fadein_blue-cloud-left 1s ease-in;
                -moz-animation: fadein_blue-cloud-left 1s ease-in;
                -ms-animation: fadein_blue-cloud-left 1s ease-in;
                -o-animation: fadein_blue-cloud-left 1s ease-in;
                animation: fadein_blue-cloud-left 1s ease-in;
            }

            @keyframes fadein_blue-cloud-left {
                from {
                    opacity: 0;
                    left: -13.33%;
                    top: 43.83%;
                }

                to {
                    opacity: 1;
                    left: 0;
                    top: 43.83%;
                }
            }

            @-moz-keyframes fadein_blue-cloud-left {
                from {
                    opacity: 0;
                    left: -13.33%;
                    top: 43.83%;
                }

                to {
                    opacity: 1;
                    left: 0;
                    top: 43.83%;
                }
            }

            @-webkit-keyframes fadein_blue-cloud-left {
                from {
                    opacity: 0;
                    left: -13.33%;
                    top: 43.83%;
                }

                to {
                    opacity: 1;
                    left: 0;
                    top: 43.83%;
                }
            }

            @-ms-keyframes fadein_blue-cloud-left {
                from {
                    opacity: 0;
                    left: -13.33%;
                    top: 43.83%;
                }

                to {
                    opacity: 1;
                    left: 0;
                    top: 43.83%;
                }
            }

            #blue-cloud-right {
                position: absolute;
                left: 87.33%;
                top: 24.65%;
                width: 11.33%;
                height: 9.04%;
                z-index: 1000;
                -webkit-animation: fadein_blue-cloud-right 1s ease-in;
                -moz-animation: fadein_blue-cloud-right 1s ease-in;
                -ms-animation: fadein_blue-cloud-right 1s ease-in;
                -o-animation: fadein_blue-cloud-right 1s ease-in;
                animation: fadein_blue-cloud-right 1s ease-in;
            }

            @keyframes fadein_blue-cloud-right {
                from {
                    opacity: 0;
                    left: 100.66%;
                    top: 24.65%;
                }

                to {
                    opacity: 1;
                    left: 87.33%;
                    top: 24.65%;
                }
            }

            @-moz-keyframes fadein_blue-cloud-right {
                from {
                    opacity: 0;
                    left: 100.66%;
                    top: 24.65%;
                }

                to {
                    opacity: 1;
                    left: 87.33%;
                    top: 24.65%;
                }
            }

            @-webkit-keyframes fadein_blue-cloud-right {
                from {
                    opacity: 0;
                    left: 100.66%;
                    top: 24.65%;
                }

                to {
                    opacity: 1;
                    left: 87.33%;
                    top: 24.65%;
                }
            }

            @-ms-keyframes fadein_blue-cloud-right {
                from {
                    opacity: 0;
                    left: 100.66%;
                    top: 24.65%;
                }

                to {
                    opacity: 1;
                    left: 87.33%;
                    top: 24.65%;
                }
            }

            #baloon {
                position: absolute;
                left: 25.33%;
                top: 13.69%;
                width: 12.26%;
                height: 38.08%;
                z-index: 1100;
                -webkit-animation: fadein_baloon 1s, move_baloon 1s linear 1s infinite alternate;
                -moz-animation: fadein_baloon 1s, move_baloon 1s linear 1s infinite alternate;
                -ms-animation: fadein_baloon 1s, move_baloon 1s linear 1s infinite alternate;
                -o-animation: fadein_baloon 1s, move_baloon 1s linear 1s infinite alternate;
                animation: fadein_baloon 1s, move_baloon 1s linear 1s infinite alternate;
            }

            @keyframes fadein_baloon {
                from {
                    left: 25.33%;
                    top: 8.21%;
                }

                to {
                    left: 25.33%;
                    top: 13.69%;
                }
            }

            @-moz-keyframes fadein_baloon {
                from {
                    left: 25.33%;
                    top: 8.21%;
                }

                to {
                    left: 25.33%;
                    top: 13.69%;
                }
            }

            @-webkit-keyframes fadein_baloon {
                from {
                    left: 25.33%;
                    top: 8.21%;
                }

                to {
                    left: 25.33%;
                    top: 13.69%;
                }
            }

            @-ms-keyframes fadein_baloon {
                from {
                    left: 25.33%;
                    top: 8.21%;
                }

                to {
                    left: 25.33%;
                    top: 13.69%;
                }
            }

            @keyframes move_baloon {
                from {
                    top: 13.69%;
                }

                to {
                    top: 15.34%;
                }
            }

            @-moz-keyframes move_baloon {
                from {
                    top: 13.69%;
                }

                to {
                    top: 15.34%;
                }
            }

            @-webkit-keyframes move_baloon {
                from {
                    top: 13.69%;
                }

                to {
                    top: 15.34%;
                }
            }

            @-ms-keyframes move_baloon {
                from {
                    top: 13.69%;
                }

                to {
                    top: 15.34%;
                }
            }
        </style>

    </head>

    <body>
        <div id="logo-container">
            <img id="logo" src="/TenantLogo.ashx?logotype=2&general=false" alt="" />
        </div>
        <div id="container">
            <svg id="background" width="753" height="361" viewBox="0 0 753 361" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M605.159 329.5L502.298 335.217C254.439 335.217 356.184 345.596 268.916 345.596C179.954 345.596 145.188 350.392 79.3741 349.916C17.1668 345.596 -36.4313 231.143 31.8546 171.934C109.509 90.4003 120.656 5.02279e-06 254.764 0C357.493 -3.84752e-06 383.499 65.6633 577.897 46.6555C712.51 33.4934 779.566 260.925 742.919 335.217C713.53 394.796 605.159 329.5 605.159 329.5Z" fill="#E9F7FF" />
            </svg>
            <svg id="birds" width="267" height="116" viewBox="0 0 267 116" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M53.0374 0C52.4644 0 52 0.457978 52 1.02292C52 1.58787 52.4644 2.04584 53.0374 2.04584C57.1445 2.04584 60.1955 5.68928 61.3682 9.2816C61.5035 9.70858 61.9047 10 62.3585 10C62.8124 10 63.2137 9.70875 63.3489 9.28152L63.3492 9.28052C64.4188 5.99204 67.0823 2.04584 70.9626 2.04584C71.5356 2.04584 72 1.58787 72 1.02292C72 0.457978 71.5356 0 70.9626 0C66.9479 0 64.0346 3.08772 62.3445 6.38386C60.4819 2.8992 57.243 0 53.0374 0Z" fill="#BADAF4"/>
                <path d="M27.193 29C26.5341 29 26 29.5496 26 30.2275C26 30.9054 26.5341 31.455 27.193 31.455C31.9162 31.455 35.4248 35.8271 36.7734 40.1379C36.929 40.6503 37.3905 41 37.9123 41C38.4343 41 38.8957 40.6505 39.0512 40.1378L39.0516 40.1366C40.2816 36.1905 43.3447 31.455 47.807 31.455C48.4659 31.455 49 30.9054 49 30.2275C49 29.5496 48.4659 29 47.807 29C43.1901 29 39.8397 32.7053 37.8962 36.6606C35.7542 32.479 32.0294 29 27.193 29Z" fill="#BADAF4"/>
                <path d="M1.05805 6C0.473705 6 0 6.43625 0 6.97439C0 7.51254 0.473705 7.94879 1.05805 7.94879C2.82976 7.94879 4.09627 8.92764 4.98064 10.0667C5.50094 10.7369 5.94102 11.5022 6.22407 12.3052L6.22539 12.3091L6.22581 12.3103C6.36174 12.72 6.77235 13 7.23768 13C7.68398 13 8.10505 12.7321 8.25168 12.3041C8.53297 11.5005 8.95254 10.7294 9.45294 10.0433C10.3046 8.87553 11.4534 7.94879 12.9419 7.94879C13.5263 7.94879 14 7.51254 14 6.97439C14 6.43625 13.5263 6 12.9419 6C10.39 6 8.68661 7.59887 7.6963 8.95671C7.52095 9.19713 7.36273 9.43672 7.22099 9.66845C7.06749 9.4293 6.89487 9.18168 6.702 8.93327C5.62553 7.54675 3.80222 6 1.05805 6Z" fill="#BADAF4"/>
                <path d="M247 107.023C247 106.458 247.464 106 248.037 106C252.243 106 255.482 108.899 257.345 112.384C259.035 109.088 261.948 106 265.963 106C266.536 106 267 106.458 267 107.023C267 107.588 266.536 108.046 265.963 108.046C262.082 108.046 259.419 111.992 258.349 115.281L258.349 115.282C258.214 115.709 257.812 116 257.359 116C256.905 116 256.504 115.709 256.368 115.282C255.196 111.689 252.144 108.046 248.037 108.046C247.464 108.046 247 107.588 247 107.023Z" fill="#BADAF4"/>
            </svg>
            <svg id="mountain-left" width="191" height="130" viewBox="0 0 191 130" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path fill-rule="evenodd" clip-rule="evenodd" d="M100.787 5.63419L179.184 110.875C181.935 114.568 190.942 119.551 190.942 119.551C190.942 119.551 148.016 118.255 88.8323 126.895C29.6489 135.535 0 119.234 0 119.234C0 119.234 3.8778 114.47 6.75659 110.778L88.8323 5.53793C91.5955 1.99477 98.1152 2.04727 100.787 5.63419Z" fill="#E1C6C6" />
                <path fill-rule="evenodd" clip-rule="evenodd" d="M95.5 5.53794C121 36.912 127.5 87.412 159 119.234C159 119.234 148.016 118.255 88.8323 126.895C29.6489 135.535 0 119.234 0 119.234C0 119.234 3.8778 114.47 6.75659 110.778L88.8323 5.53794C91.5955 1.99478 92.828 1.95102 95.5 5.53794Z" fill="#E6CECE" />
                <path d="M108.5 33.912C113 28.412 120.893 33.1437 120 30.912C119 28.412 110.66 17.004 108.5 14.412L99.5 3.41202C95.5 -1.08798 90 0.412003 85 7.41202L78.5 15.912C74.5 20.9261 66.5 32.912 66.5 32.912C77.5 22.912 82.1015 32.5223 87 27.912C95.5 19.912 102.003 41.8522 108.5 33.912Z" fill="white" />
            </svg>
            <svg id="mountain-right" width="230" height="162" viewBox="0 0 230 162" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path fill-rule="evenodd" clip-rule="evenodd" d="M129.056 6.27931L222.861 131.807C226.153 136.213 229.641 144.1 229.641 144.1C229.641 144.1 208.905 171.748 121.642 156.196C34.3791 140.644 0.251465 156.196 0.251465 156.196C0.251465 156.196 13.0994 136.095 16.544 131.692L114.751 6.16449C118.058 1.93832 125.859 2.00093 129.056 6.27931Z" fill="#B59C9C" />
                <path fill-rule="evenodd" clip-rule="evenodd" d="M129.056 6.27931C143.5 57.412 184.124 100.94 206 150.412C206 150.412 208.905 171.748 121.642 156.196C34.3791 140.644 0.251465 156.196 0.251465 156.196C0.251465 156.196 13.0994 136.095 16.544 131.692L114.751 6.16449C118.058 1.93832 125.859 2.00093 129.056 6.27931Z" fill="#BCA7A7" />
                <path d="M109 39.912C105.361 43.8211 104 29.9121 95.5 28.912C95.5 28.912 96.2357 26.3398 98.1979 23.9575L114.845 3.03197C117.929 -0.71246 124.826 -0.603776 127.724 3.23494L145.223 26.4121C146.136 27.6214 151.854 34.2395 151.018 35.484C144 35.484 138.674 23.9041 132 31.412C124 40.412 122.5 25.412 109 39.912Z" fill="white" />
            </svg>
            <svg id="mountain-center" width="364" height="242" viewBox="0 0 364 242" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path fill-rule="evenodd" clip-rule="evenodd" d="M196.843 8.54528L345.932 200.731C351.165 207.476 363.298 222.596 363.298 222.596C363.298 222.596 293.315 257.164 180.564 230.372C67.8133 203.579 5.17395 215.997 5.17395 215.997C5.17395 215.997 12.5485 207.296 18.0231 200.555L174.108 8.36949C179.363 1.89911 191.762 1.99498 196.843 8.54528Z" fill="#FAE0D2" />
                <path fill-rule="evenodd" clip-rule="evenodd" d="M192 5.77758C251.837 82.912 281 186.912 333.923 232.532C333.923 232.532 270.851 252.836 174.108 232.532C77.3655 212.228 0.854004 219.14 0.854004 219.14C58.5157 151.11 117.888 75.0012 174.108 5.77758C179.363 -0.6928 186.919 -0.772726 192 5.77758Z" fill="#FFE9DD" />
                <path d="M147.732 91.7013C137.365 88.6773 149.808 61.2481 120 72.912C112.211 69.8585 151.512 30.5027 168.9 9.16558C177.108 -0.263733 185.748 -4.83696 196.98 7.46215C213.582 28.4184 249.705 64.8585 245.5 71.412C223.9 62.3926 252.154 99.9491 217.284 78.6262C204.756 70.9655 202.164 59.7336 196.98 59.7336C191.796 59.7336 186.18 76.1494 173.652 76.1494C161.124 76.1494 158.1 94.7252 147.732 91.7013Z" fill="white" />
            </svg>
            <svg id="white-cloud-behind" width="63" height="27" viewBox="0 0 63 27" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path fill-rule="evenodd" clip-rule="evenodd" d="M62.0283 26.8523H1.63598C1.08018 25.5954 0.771484 24.2047 0.771484 22.7419C0.771484 17.8782 4.18439 13.8116 8.74599 12.8063C10.407 5.47421 16.9629 0 24.7974 0C30.5416 0 35.5985 2.94278 38.5427 7.40298C40.0926 6.52762 41.883 6.02808 43.79 6.02808C49.6918 6.02808 54.4762 10.8124 54.4762 16.7142C54.4762 17.3503 54.4206 17.9734 54.314 18.579C54.5181 18.5615 54.7246 18.5526 54.9332 18.5526C58.8995 18.5526 62.1148 21.768 62.1148 25.7343C62.1148 26.1146 62.0853 26.488 62.0283 26.8523Z" fill="white" />
            </svg>
            <svg id="white-cloud-center" width="74" height="33" viewBox="0 0 74 33" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path fill-rule="evenodd" clip-rule="evenodd" d="M73.3357 32.1895H1.03496C0.369565 30.6847 0 29.0198 0 27.2686C0 21.4458 4.08587 16.5773 9.54695 15.3738C11.5355 6.59599 19.3841 0.0423584 28.7634 0.0423584C35.6403 0.0423584 41.6943 3.5654 45.219 8.90507C47.0746 7.8571 49.218 7.25906 51.5011 7.25906C58.5666 7.25906 64.2943 12.9868 64.2943 20.0523C64.2943 20.8138 64.2278 21.5598 64.1002 22.2847C64.3445 22.2639 64.5918 22.2532 64.8415 22.2532C69.5899 22.2532 73.4392 26.1026 73.4392 30.851C73.4392 31.3063 73.4038 31.7533 73.3357 32.1895Z" fill="white" />
            </svg>
            <svg id="white-cloud-left" width="180" height="80" viewBox="0 0 180 80" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path fill-rule="evenodd" clip-rule="evenodd" d="M178.952 79.5991H3.02637C1.08826 75.6491 0 71.2064 0 66.5096C0 50.0768 13.3215 36.7553 29.7543 36.7553C31.2286 36.7553 32.6778 36.8625 34.0946 37.0696C39.0926 15.8192 58.1735 0 80.9492 0C103.664 0 122.703 15.7345 127.763 36.8987C138.074 38.9576 146.476 46.3064 150.02 55.9956C152.586 54.8778 155.42 54.2579 158.398 54.2579C169.998 54.2579 179.401 63.6613 179.401 75.2609C179.401 76.7482 179.246 78.1993 178.952 79.5991Z" fill="white" />
            </svg>
            <svg id="white-cloud-right" width="160" height="70" viewBox="0 0 160 70" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path fill-rule="evenodd" clip-rule="evenodd" d="M159.613 69.9674H2.25259C0.80436 66.6922 0 63.0686 0 59.2571C0 46.584 8.89278 35.9879 20.7787 33.3685C25.1067 14.2638 42.189 0 62.6028 0C77.5701 0 90.7464 7.6678 98.4179 19.2894C102.457 17.0086 107.122 15.707 112.091 15.707C127.468 15.707 139.935 28.1732 139.935 43.5511C139.935 45.2085 139.79 46.8321 139.512 48.4099C140.044 48.3645 140.582 48.3413 141.126 48.3413C151.46 48.3413 159.838 56.7193 159.838 67.0541C159.838 68.0451 159.761 69.0181 159.613 69.9674Z" fill="white" />
            </svg>
            <svg id="blue-cloud-left" width="63" height="24" viewBox="0 0 63 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path fill-rule="evenodd" clip-rule="evenodd" d="M62.1979 23.7708H0C0.135632 10.6194 10.8389 0 24.0224 0C32.5025 0 39.9564 4.3938 44.2317 11.029C45.7258 10.4393 47.3539 10.1152 49.0576 10.1152C56.3201 10.1152 62.2074 16.0026 62.2074 23.265C62.2074 23.4344 62.2042 23.603 62.1979 23.7708Z" fill="#D0E7F9" />
            </svg>
            <svg id="blue-cloud-right" width="85" height="33" viewBox="0 0 85 33" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path fill-rule="evenodd" clip-rule="evenodd" d="M0.011907 32.1712H84.4586C83.3204 14.213 68.3946 0 50.1493 0C37.9859 0 27.2979 6.31668 21.1866 15.8486C19.6727 15.3875 18.0658 15.1394 16.401 15.1394C7.34299 15.1394 0 22.4824 0 31.5404C0 31.7516 0.00399277 31.9619 0.011907 32.1712Z" fill="#D0E7F9" />
            </svg>
            <svg id="baloon" width="92" height="139" viewBox="0 0 92 139" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M4.75806 61.9671L46.4736 135.739M46.4736 135.739L22.9019 80.111M46.4736 135.739L27.6538 37.7754M46.4736 135.739L65.2374 37.7754M46.4736 135.739L70.4214 80.111M46.4736 135.739L88.5652 61.9671M46.4736 135.739V56.3512" stroke="#CBE4F6" />
                <path d="M40.1818 126.334H52.2776V135.43C52.2776 137.087 50.9345 138.43 49.2776 138.43H43.1818C41.5249 138.43 40.1818 137.087 40.1818 135.43V126.334Z" fill="#BCA7A7" />
                <rect x="39.3177" y="125.47" width="13.8239" height="1.72798" fill="#CBE4F6" />
                <rect x="41.9097" y="128.926" width="1.72798" height="6.91193" fill="#E1C6C6" />
                <rect x="45.3657" y="128.926" width="1.72798" height="6.91193" fill="#E1C6C6" />
                <rect x="48.8217" y="128.926" width="1.72798" height="6.91193" fill="#E1C6C6" />
                <circle cx="46" cy="46" r="46" fill="#D0E7F9" />
                <ellipse cx="46" cy="46" rx="30" ry="46" fill="#E9F7FF" />
                <ellipse cx="46" cy="46" rx="12" ry="46" fill="#D0E7F9" />
            </svg>
        </div>
        <h1 id="header">Error 403. Access forbidden</h1>
        <p id="text">You don’t have permission to access this page.</p>
        <div id="button-container"></div>
    </body>
</html>

<% 
    Response.StatusCode = 403;
    Response.TrySkipIisCustomErrors = true;
 %>