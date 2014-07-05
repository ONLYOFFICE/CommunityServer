/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

// // --------------------------------------------------------------------------------------------------------------------
// // <copyright company="Ascensio System Limited" file="GeoLoc.cs">
// //   
// // </copyright>
// // <summary>
// //   (c) Copyright Ascensio System Limited 2008-2012
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------

using System;
using ASC.Xmpp.Core.utils;
using ASC.Xmpp.Core.utils.Xml.Dom;

namespace ASC.Xmpp.Core.protocol.extensions.geoloc
{
    /*
    
    Element Name  	Inclusion  	Datatype  	    Definition
    
    alt 	        MAY 	    xs:decimal 	    Altitude in meters above or below sea level
    bearing 	    MAY 	    xs:decimal 	    GPS bearing (direction in which the entity is heading to reach its next waypoint), measured in decimal degrees relative to true north [2]
    datum 	        MAY 	    xs:string 	    GPS datum [3]
    description 	MAY 	    xs:string 	    A natural-language description of the location
    error 	        MAY 	    xs:decimal 	    Horizontal GPS error in arc minutes
    lat 	        MUST 	    xs:decimal 	    Latitude in decimal degrees North
    lon 	        MUST 	    xs:decimal 	    Longitude in decimal degrees East
    timestamp 	    MAY 	    xs:datetime 	UTC timestamp specifying the moment when the reading was taken (MUST conform to the DateTime profile of Jabber Date and Time Profiles [4])
           
    */

    /// <summary>
    ///   XEP-0080 Geographical Location (GeoLoc) This JEP defines a format for capturing data about an entity's geographical location (geoloc). The namespace defined herein is intended to provide a semi-structured format for describing a geographical location that may change fairly frequently, where the geoloc information is provided as Global Positioning System (GPS) coordinates.
    /// </summary>
    public class GeoLoc : Element
    {
        #region << Constructors >>

        /// <summary>
        /// </summary>
        public GeoLoc()
        {
            TagName = "geoloc";
            Namespace = Uri.GEOLOC;
        }

        /// <summary>
        /// </summary>
        /// <param name="Latitude"> </param>
        /// <param name="Longitude"> </param>
        public GeoLoc(double latitude, double longitude) : this()
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        #endregion

        /// <summary>
        ///   A natural-language description of the location
        /// </summary>
        public string Description
        {
            get { return GetTag("description"); }
            set { SetTag("description", value); }
        }

        /// <summary>
        ///   GPS datum
        /// </summary>
        public string Datum
        {
            get { return GetTag("datum"); }
            set { SetTag("datum", value); }
        }

        /// <summary>
        ///   Latitude in decimal degrees North
        /// </summary>
        public double Latitude
        {
            get { return GetTagDouble("lat"); }
            set { SetTag("lat", value); }
        }

        /// <summary>
        ///   Longitude in decimal degrees East
        /// </summary>
        public double Longitude
        {
            get { return GetTagDouble("lon"); }
            set { SetTag("lon", value); }
        }

        /// <summary>
        ///   Altitude in meters above or below sea level
        /// </summary>
        public double Altitude
        {
            get { return GetTagDouble("alt"); }
            set { SetTag("alt", value); }
        }

        /// <summary>
        ///   GPS bearing (direction in which the entity is heading to reach its next waypoint), measured in decimal degrees relative to true north
        /// </summary>
        public double Bearing
        {
            get { return GetTagDouble("bearing"); }
            set { SetTag("bearing", value); }
        }

        /// <summary>
        ///   Horizontal GPS error in arc minutes
        /// </summary>
        public double Error
        {
            get { return GetTagDouble("error"); }
            set { SetTag("error", value); }
        }

        /// <summary>
        ///   UTC timestamp specifying the moment when the reading was taken
        /// </summary>
        public DateTime Timestamp
        {
            get { return Time.ISO_8601Date(GetTag("timestamp")); }
            set { SetTag("timestamp", Time.ISO_8601Date(value)); }
        }
    }
}