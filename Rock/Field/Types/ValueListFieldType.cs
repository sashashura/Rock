﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a key/value list
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.VALUE_LIST )]
    public class ValueListFieldType : FieldType, IEntityReferenceFieldType
    {

        #region Configuration

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var values = privateValue?.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToArray() ?? new string[0];
            values = values.Select( s => HttpUtility.UrlDecode( s ) ).ToArray();

            if ( privateConfigurationValues != null && privateConfigurationValues.ContainsKey( "definedtype" ) )
            {
                if ( Int32.TryParse( privateConfigurationValues["definedtype"], out int definedTypeId ) )
                {
                    for ( int i = 0; i < values.Length; i++ )
                    {
                        var definedValue = DefinedValueCache.Get( values[i].AsInteger() );
                        if ( definedValue != null )
                        {
                            values[i] = definedValue.Value;
                        }
                    }
                }
            }

            return values.ToList().AsDelimited( ", " );
        }

        #endregion

        #region Edit Control

        #endregion

        #region Filter Control

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion

        #region Persistence

        /// <inheritdoc/>
        public override bool IsPersistedValueInvalidated( Dictionary<string, string> oldPrivateConfigurationValues, Dictionary<string, string> newPrivateConfigurationValues )
        {
            var oldDefinedtype = oldPrivateConfigurationValues.GetValueOrNull( "definedtype" ) ?? string.Empty;
            var newDefinedtype = newPrivateConfigurationValues.GetValueOrNull( "definedtype" ) ?? string.Empty;

            return oldDefinedtype != newDefinedtype;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var values = privateValue?.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) ?? new string[0];
            values = values.Select( s => HttpUtility.UrlDecode( s ) ).ToArray();

            if ( privateConfigurationValues != null && privateConfigurationValues.ContainsKey( "definedtype" ) )
            {
                if ( Int32.TryParse( privateConfigurationValues["definedtype"], out int definedTypeId ) )
                {
                    return values.Select( v => v.AsIntegerOrNull() )
                        .Where( id => id.HasValue )
                        .Select( id => DefinedValueCache.Get( id.Value ) )
                        .Where( dv => dv != null )
                        .Select( dv => new ReferencedEntity( EntityTypeCache.GetId<DefinedValue>().Value, dv.Id ) )
                        .ToList();
                }
            }

            return null;
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a Defined Value and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<DefinedValue>().Value, nameof( DefinedValue.Value ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( "valueprompt" );
            configKeys.Add( "definedtype" );
            configKeys.Add( "customvalues" );
            configKeys.Add( "allowhtml" );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var tbValuePrompt = new RockTextBox();
            controls.Add( tbValuePrompt );
            tbValuePrompt.AutoPostBack = true;
            tbValuePrompt.TextChanged += OnQualifierUpdated;
            tbValuePrompt.Label = "Label Prompt";
            tbValuePrompt.Help = "The text to display as a prompt in the label textbox.";

            var ddl = new RockDropDownList();
            controls.Add( ddl );
            ddl.AutoPostBack = true;
            ddl.SelectedIndexChanged += OnQualifierUpdated;
            ddl.DataTextField = "Name";
            ddl.DataValueField = "Id";
            ddl.DataSource = new Rock.Model.DefinedTypeService( new RockContext() ).Queryable().OrderBy( d => d.Order ).ToList();
            ddl.DataBind();
            ddl.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
            ddl.Label = "Defined Type";
            ddl.Help = "Optional Defined Type to select values from, otherwise values will be free-form text fields.";

            var tbCustomValues = new RockTextBox();
            controls.Add( tbCustomValues );
            tbCustomValues.TextMode = TextBoxMode.MultiLine;
            tbCustomValues.Rows = 3;
            tbCustomValues.AutoPostBack = true;
            tbCustomValues.TextChanged += OnQualifierUpdated;
            tbCustomValues.Label = "Custom Values";
            tbCustomValues.Help = "Optional list of options to use for the values.  Format is either 'value1,value2,value3,...', or 'value1^text1,value2^text2,value3^text3,...'.";

            var cbAllowHtml = new RockCheckBox();
            controls.Add( cbAllowHtml );

            // turn on AutoPostBack so that it'll create the Editor control based on AllowHtml
            cbAllowHtml.AutoPostBack = true;
            cbAllowHtml.Label = "Allow HTML";
            cbAllowHtml.Help = "Allow HTML content in values.";

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( "valueprompt", new ConfigurationValue( "Label Prompt", "The text to display as a prompt in the label textbox.", "" ) );
            configurationValues.Add( "definedtype", new ConfigurationValue( "Defined Type", "Optional Defined Type to select values from, otherwise values will be free-form text fields", "" ) );
            configurationValues.Add( "customvalues", new ConfigurationValue( "Custom Values", "Optional list of options to use for the values.  Format is either 'value1,value2,value3,...', or 'value1^text1,value2^text2,value3^text3,...'.", "" ) );
            configurationValues.Add( "allowhtml", new ConfigurationValue( "Allow HTML", "Allow HTML content in values", "" ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is RockTextBox )
                {
                    configurationValues["valueprompt"].Value = ( ( RockTextBox ) controls[0] ).Text;
                }
                if ( controls.Count > 1 && controls[1] != null && controls[1] is RockDropDownList )
                {
                    configurationValues["definedtype"].Value = ( ( RockDropDownList ) controls[1] ).SelectedValue;
                }
                if ( controls.Count > 2 && controls[2] != null && controls[2] is RockTextBox )
                {
                    configurationValues["customvalues"].Value = ( ( RockTextBox ) controls[2] ).Text;
                }
                if ( controls.Count > 3 && controls[3] != null && controls[3] is RockCheckBox )
                {
                    configurationValues["allowhtml"].Value = ( ( RockCheckBox ) controls[3] ).Checked.ToTrueFalse();
                }
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && configurationValues != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is RockTextBox && configurationValues.ContainsKey( "valueprompt" ) )
                {
                    ( ( RockTextBox ) controls[0] ).Text = configurationValues["valueprompt"].Value;
                }
                if ( controls.Count > 1 && controls[1] != null && controls[1] is RockDropDownList && configurationValues.ContainsKey( "definedtype" ) )
                {
                    ( ( RockDropDownList ) controls[1] ).SelectedValue = configurationValues["definedtype"].Value;
                }
                if ( controls.Count > 2 && controls[2] != null && controls[2] is RockTextBox && configurationValues.ContainsKey( "customvalues" ) )
                {
                    ( ( RockTextBox ) controls[2] ).Text = configurationValues["customvalues"].Value;
                }
                if ( controls.Count > 3 && controls[3] != null && controls[3] is RockCheckBox && configurationValues.ContainsKey( "allowhtml" ) )
                {
                    ( ( RockCheckBox ) controls[3] ).Checked = configurationValues["allowhtml"].Value.AsBoolean();
                }
            }
        }

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues"></param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <summary>
        /// Edits the control.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public virtual ValueList EditControl( string id )
        {
            return new ValueList { ID = id };
        }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var control = EditControl( id );

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( "valueprompt" ) )
                {
                    control.ValuePrompt = configurationValues["valueprompt"].Value;
                }
                if ( configurationValues.ContainsKey( "definedtype" ) )
                {
                    int definedTypeId = 0;
                    if ( Int32.TryParse( configurationValues["definedtype"].Value, out definedTypeId ) )
                    {
                        control.DefinedTypeId = definedTypeId;
                    }
                }

                if ( configurationValues.ContainsKey( "customvalues" ) )
                {
                    control.CustomValues = Helper.GetConfiguredValues( configurationValues, "customvalues" );
                }

                if ( control is ValueList && configurationValues.ContainsKey( "allowhtml" ) )
                {
                    ( control as ValueList ).AllowHtmlValue = configurationValues["allowhtml"].Value.AsBoolean();
                }
            }

            return control;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as ValueList;
            if ( picker != null )
            {
                return picker.Value;
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as ValueList;
            if ( picker != null )
            {
                picker.Value = value;
            }
        }

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            // This field type does not support filtering
            return null;
        }

#endif
        #endregion
    }
}