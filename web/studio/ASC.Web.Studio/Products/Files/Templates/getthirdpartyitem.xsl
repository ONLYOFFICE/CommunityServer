<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />

  <register type="ASC.Web.Files.Resources.FilesCommonResource,ASC.Web.Files" alias="fres" />

  <xsl:template match="third_partyList">
    <xsl:for-each select="entry">
      <div class="borderBase account-row">
        <xsl:attribute name="id">account_<xsl:value-of select="provider_id"/></xsl:attribute>
        <div class="account-header-container">
          <xsl:if test="isNew = 'false'">
            <div class="menu-small">
              <xsl:attribute name="title">
                <resource name="fres.TitleShowFolderActions" />
              </xsl:attribute>
            </div>
          </xsl:if>
          <div>
            <xsl:attribute name="class">
              account-icon <xsl:value-of select="provider_key"/>
            </xsl:attribute>
          </div>
          <div class="account-provider-title">
            <xsl:value-of select="provider_title"/>
          </div>
          <xsl:if test="isNew = 'false'">
            <div class="account-customer-title">
              <a class="link">
                <xsl:choose>
                  <xsl:when test="link != ''">
                    <xsl:attribute name="href">
                      <xsl:value-of select="link"/>
                    </xsl:attribute>
                    <xsl:attribute name="target">_blank</xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="href">
                      #<xsl:value-of select="id"/>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:attribute name="title">
                  <xsl:value-of select="customer_title"/>
                </xsl:attribute>
                <xsl:value-of select="customer_title"/>
              </a>
            </div>
          </xsl:if>
          <xsl:if test="error != ''">
            <div class="account-provider-error">
              <xsl:attribute name="title">
                <xsl:value-of select="error"/>
              </xsl:attribute>
              <xsl:value-of select="error"/>
            </div>
          </xsl:if>
        </div>
        <xsl:if test="canEdit = 'true'">
          <div class="account-settings-container">
            <div class="account-log-pass-container">
              <div class="account-field-url account-field-row">
                <div class="account-field-title">
                  <resource name="fres.ConnectionUrl" />
                </div>
                <div class="account-field-body">
                  <input type="url" class="textEdit account-input-url" name="account-field" autocomplete="off">
                    <xsl:if test="isNew != 'true'">
                      <xsl:attribute name="placeholder">
                        <resource name="fres.ThirdPartyCorrect"/>
                      </xsl:attribute>
                    </xsl:if>
                  </input>
                </div>
              </div>
              <xsl:if test="isNew = 'true'">
                <div class="account-field-row">
                  <div class="account-field-title">
                    <resource name="fres.Login" />
                  </div>
                  <div class="account-field-body">
                    <input type="text" class="textEdit account-input-login" name="account-field" autocomplete="off"/>
                  </div>
                </div>
              </xsl:if>
              <div class="account-field-row">
                <div class="account-field-title">
                  <resource name="fres.Password" />
                </div>
                <div class="account-field-body">
                  <input type="password" class="textEdit account-input-pass" name="account-field" autocomplete="new-password">
                    <xsl:if test="isNew != 'true'">
                      <xsl:attribute name="placeholder">
                        <resource name="fres.ThirdPartyCorrect"/>
                      </xsl:attribute>
                    </xsl:if>
                  </input>
                </div>
              </div>
            </div>
            <div class="account-folder-settings-container">
              <xsl:if test="getTokenUrl != ''">
                <div class="account-field-row">
                  <div class="account-field-title">
                    <resource name="fres.ThirdPartyReconnectTitle" />
                  </div>
                  <div class="account-field-body">
                    <div class="edit-account-button button white">
                      <xsl:attribute name="data-provider">
                        <xsl:value-of select="provider_key"/>
                      </xsl:attribute>
                      <span>
                        <xsl:attribute name="class">
                          <xsl:value-of select="provider_key"/>
                        </xsl:attribute>
                      </span>
                      <resource name="fres.ThirdPartyReconnect" />
                    </div>
                  </div>
                </div>
              </xsl:if>
              <div>
                <xsl:attribute name="class">
                  account-field-row <xsl:if test="canCorporate = 'true'">account-field-row-large</xsl:if>
                </xsl:attribute>
                <div class="account-field-title">
                  <resource name="fres.ThirdPartyFolderTitle" />
                </div>
                <div class="account-field-body">
                  <input type="text" class="textEdit account-input-folder">
                    <xsl:attribute name="maxlength">
                      <xsl:value-of select="max_name_length"/>
                    </xsl:attribute>
                    <xsl:attribute name="value">
                      <xsl:value-of select="customer_title"/>
                    </xsl:attribute>
                    <xsl:attribute name="placeholder">
                      <xsl:value-of select="customer_title"/>
                    </xsl:attribute>
                  </input>
                  <xsl:if test="canCorporate = 'true'">
                    <label>
                      <input type="checkbox" class="account-cbx-corporate checkbox">
                        <xsl:if test="corporate = 'true'">
                          <xsl:attribute name="checked">checked</xsl:attribute>
                        </xsl:if>
                      </input>
                      <resource name="fres.ThirdPartySetCorporate" />
                    </label>
                  </xsl:if>
                </div>
              </div>
              <div class="account-action-container">
                <a class="button middle blue account-save-link">
                  <resource name="fres.ButtonSave"/>
                </a>
                <span class="splitter-buttons"></span>
                <a class="button middle gray account-cancel-link">
                  <resource name="fres.ButtonCancel"/>
                </a>
              </div>
            </div>
          </div>
        </xsl:if>
        <input type="hidden" class="account-hidden-provider-id">
          <xsl:attribute name="value">
            <xsl:value-of select="provider_id"/>
          </xsl:attribute>
        </input>
        <input type="hidden" class="account-hidden-provider-key">
          <xsl:attribute name="value">
            <xsl:value-of select="provider_key"/>
          </xsl:attribute>
        </input>
        <input type="hidden" class="account-hidden-provider-title">
          <xsl:attribute name="value">
            <xsl:value-of select="provider_title"/>
          </xsl:attribute>
        </input>
        <input type="hidden" class="account-hidden-customer-title">
          <xsl:attribute name="value">
            <xsl:value-of select="customer_title"/>
          </xsl:attribute>
        </input>
        <input type="hidden" class="account-hidden-token"/>
      </div>
    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>