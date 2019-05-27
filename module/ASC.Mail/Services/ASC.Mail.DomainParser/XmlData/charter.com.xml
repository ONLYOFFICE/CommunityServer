<clientConfig version="1.1">
  <emailProvider id="charter.com">
    <domain>charter.net</domain>
    <domain>charter.com</domain>
    <displayName>Charter Commuications</displayName>
    <displayShortName>Charter</displayShortName>
    <incomingServer type="imap">
      <hostname>mobile.charter.net</hostname>
      <port>993</port>
      <socketType>SSL</socketType>
      <username>%EMAILADDRESS%</username>
      <authentication>password-cleartext</authentication>
    </incomingServer>
    <incomingServer type="imap">
      <hostname>imap.charter.net</hostname>
      <port>143</port>
      <socketType>plain</socketType>
      <username>%EMAILADDRESS%</username>
      <authentication>password-cleartext</authentication>
    </incomingServer>
    <!-- We can't in good conscience recommend an insecure config while a secure config is available.
    <incomingServer type="pop3">
      <hostname>pop.charter.net</hostname>
      <port>110</port>
      <socketType>plain</socketType>
      <username>%EMAILADDRESS%</username>
      <authentication>password-cleartext</authentication>
    </incomingServer>
    -->

    <outgoingServer type="smtp">
      <hostname>mobile.charter.net</hostname>
      <!-- Yes, they have SSL (not STARTTLS, not plain SMTP) on port 587. Spec violation. -->
      <port>587</port>
      <socketType>SSL</socketType>
      <username>%EMAILADDRESS%</username>
      <authentication>password-cleartext</authentication>
    </outgoingServer>
    <outgoingServer type="smtp">
      <hostname>smtp.charter.net</hostname>
      <port>25</port>
      <socketType>plain</socketType>
      <username>%EMAILADDRESS%</username>
      <authentication>password-cleartext</authentication>
      <restriction>client-IP-address</restriction>
    </outgoingServer>
    <documentation url="http://www.myaccount.charter.com/customers/support.aspx?supportarticleid=1417">
      <descr>POP/IMAP and SMTP settings for Charter, including mobile</descr>
    </documentation>

  </emailProvider>
</clientConfig>
