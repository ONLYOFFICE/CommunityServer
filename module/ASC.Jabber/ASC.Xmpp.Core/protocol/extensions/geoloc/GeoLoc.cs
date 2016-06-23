/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


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