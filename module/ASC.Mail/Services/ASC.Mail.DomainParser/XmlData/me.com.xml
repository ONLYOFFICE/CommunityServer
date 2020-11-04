<clientConfig version="1.0">
  <emailProvider id="me.com">
    <domain>mac.com</domain>
    <domain>me.com</domain>
    <displayName>Apple iCloud</displayName>
    <displayShortName>Apple</displayShortName>
    <incomingServer type="imap">
      <hostname>imap.mail.me.com</hostname>
      <port>993</port>
      <socketType>SSL</socketType>
      <username>%EMAILLOCALPART%</username>
      <authentication>plain</authentication>
    </incomingServer>
    <!-- Not working for me. BenB 2010-08-19 from Europe
    <outgoingServer type="smtp">
      <hostname>smtp.me.com</hostname>
      <port>465</port>
      <socketType>SSL</socketType>
      <username>%EMAILLOCALPART%</username>
      <authentication>password-cleartext</authentication>
    </outgoingServer>
    -->
    <outgoingServer type="smtp">
      <hostname>smtp.mail.me.com</hostname>
      <port>587</port>
      <socketType>STARTTLS</socketType>
      <username>%EMAILLOCALPART%</username>
      <authentication>plain</authentication>
    </outgoingServer>
  </emailProvider>
</clientConfig>