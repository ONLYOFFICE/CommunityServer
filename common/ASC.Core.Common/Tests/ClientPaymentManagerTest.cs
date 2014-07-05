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

#if DEBUG
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ASC.Core.Common.Tests
{
    [TestClass]
    public class ClientPaymentManagerTest
    {
        private readonly ClientPaymentManager paymentManager = new ClientPaymentManager(null, null, null);


        [TestMethod]
        public void ActivateCuponTest()
        {
            CoreContext.TenantManager.SetCurrentTenant(0);
            paymentManager.ActivateKey("IAALKCPBRY9ZSDLJZ4E2");
        }

        [TestMethod]
        public void GetPartnerByCupon()
        {
            paymentManager.GetPartner("WRFWGF6H2S7LBVS7WB01");
        }

        [TestMethod]
        public void CreateButton()
        {
            //var url = paymentManager.GetButton(-48, "f4cd3678-4725-4826-ab50-a0704d3295c2");
        }
    }
}
#endif