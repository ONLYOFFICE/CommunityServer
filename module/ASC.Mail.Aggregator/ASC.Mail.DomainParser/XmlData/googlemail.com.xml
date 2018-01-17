<?xml version="1.0" encoding="UTF-8"?>
<clientConfig version="1.1">
  <emailProvider id="googlemail.com">
    <domain>gmail.com</domain>
    <domain>googlemail.com</domain>
    <!-- MX, for Google Apps -->
    <domain>google.com</domain>
    <!-- HACK. Only add ISPs with 100000+ users here -->
    <domain>jazztel.es</domain>
    <displayName>Google Mail</displayName>
    <displayShortName>GMail</displayShortName>
    <incomingServer type="imap">
      <hostname>imap.googlemail.com</hostname>
      <port>993</port>
      <socketType>SSL</socketType>
      <username>%EMAILADDRESS%</username>
      <authentication>password-cleartext</authentication>
    </incomingServer>
    <incomingServer type="pop3">
      <hostname>pop.googlemail.com</hostname>
      <port>995</port>
      <socketType>SSL</socketType>
      <username>%EMAILADDRESS%</username>
      <authentication>password-cleartext</authentication>
      <pop3>
        <leaveMessagesOnServer>true</leaveMessagesOnServer>
      </pop3>
    </incomingServer>
    <outgoingServer type="smtp">
      <hostname>smtp.googlemail.com</hostname>
      <port>465</port>
      <socketType>SSL</socketType>
      <username>%EMAILADDRESS%</username>
      <authentication>password-cleartext</authentication>
    </outgoingServer>
    <enable visiturl="https://mail.google.com/mail/?ui=2&amp;shva=1#settings/fwdandpop">
      <instruction>You need to enable IMAP access</instruction>
    </enable>
    <documentation url="http://mail.google.com/support/bin/answer.py?answer=13273">
      <descr>How to enable IMAP/POP3 in GMail</descr>
    </documentation>
    <documentation url="http://mail.google.com/support/bin/topic.py?topic=12806">
      <descr>How to configure email clients for IMAP</descr>
    </documentation>
    <documentation url="http://mail.google.com/support/bin/topic.py?topic=12805">
      <descr>How to configure email clients for POP3</descr>
    </documentation>
    <documentation url="http://mail.google.com/support/bin/answer.py?answer=86399">
      <descr>How to configure TB 2.0 for POP3</descr>
    </documentation>
  </emailProvider>
</clientConfig>
