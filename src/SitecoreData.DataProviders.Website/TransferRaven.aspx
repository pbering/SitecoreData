<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="Sitecore.Configuration" %>
<%@ Import Namespace="Sitecore.Data" %>
<%@ Import Namespace="Sitecore.Data.Fields" %>
<%@ Import Namespace="Sitecore.Data.Items" %>
<%@ Import Namespace="Sitecore.Globalization" %>
<%@ Import Namespace="Sitecore.SecurityModel" %>
<%@ Import Namespace="SitecoreData.DataProviders" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
    <head>
        <title>Transfer items from a different database to the raven database</title>
    </head>
    <body>
        <script runat="server">

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);

                if (!IsPostBack)
                {
                    ddlDatabase.DataSource = new object[] {"- select database -"}.Concat(Factory.GetDatabaseNames());
                    ddlDatabase.DataBind();
                }
            }

            public void DatabaseSelected(object sender, EventArgs e)
            {
                if (!"- select database -".Equals(ddlDatabase.SelectedValue))
                {
                    var database = Factory.GetDatabase(ddlDatabase.SelectedValue);
                    var raven = Factory.GetDatabase("nosqlraven");

                    if (database != null && raven != null)
                    {
                        using (new SecurityDisabler())
                        {
                            var item = database.GetRootItem();

                            var dataProvider = raven.GetDataProviders().First() as DataProviderWrapper;

                            Response.Write("<ul>");
                            Response.Flush();

                            TransferRecursive(item, dataProvider);

                            Response.Write("</ul>");
                            Response.Flush();
                        }
                    }
                }
            }

            public void TransferRecursive(Item item, DataProviderWrapper provider)
            {
                Response.Write(string.Format("<li>Transferring {0}</li>", item.Paths.FullPath));
                Response.Flush();

                ItemDefinition parentDefinition = null;

                if (item.Parent != null)
                {
                    parentDefinition = new ItemDefinition(item.Parent.ID, item.Parent.Name, item.Parent.TemplateID, item.Parent.BranchId);
                }

                // Create the item in database
                if (provider.CreateItem(item.ID, item.Name, item.TemplateID, parentDefinition, null))
                {
                    foreach (var language in item.Languages)
                    {
                        using (new LanguageSwitcher(language))
                        {
                            var itemInLanguage = item.Database.GetItem(item.ID);

                            if (itemInLanguage != null)
                            {
                                // Add a version
                                var itemDefinition = provider.GetItemDefinition(itemInLanguage.ID, null);

                                // TODO: Add all version and not just v1
                                provider.AddVersion(itemDefinition, new VersionUri(language, Sitecore.Data.Version.First), null);

                                // Send the field values to the provider
                                var changes = new ItemChanges(itemInLanguage);

                                foreach (Field field in itemInLanguage.Fields)
                                {
                                    changes.FieldChanges[field.ID] = new FieldChange(field, field.Value);
                                }

                                provider.SaveItem(itemDefinition, changes, null);
                            }
                        }
                    }
                }

                if (!item.HasChildren)
                {
                    return;
                }
                foreach (Item child in item.Children)
                {
                    TransferRecursive(child, provider);
                }
            }

        </script>
        <form runat="server">
            <p>
                Transfer items from this database (starts immediately):
                <asp:DropDownList runat="server" ID="ddlDatabase" AutoPostBack="true" OnSelectedIndexChanged="DatabaseSelected" />
            </p>
        </form>
    </body>
</html>