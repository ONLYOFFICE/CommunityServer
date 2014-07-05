if (typeof Ajax == "undefined") Ajax = {};
if (typeof Ajax.Web == "undefined") Ajax.Web = {};
if (typeof Ajax.Web.NameValueCollection == "undefined") Ajax.Web.NameValueCollection = {};

Ajax.Web.NameValueCollection = function (items) {
    this.__type = "System.Collections.Specialized.NameValueCollection";
    this.keys = [];
    this.values = [];

    if (items != null && !isNaN(items.length)) {
        for (var i = 0; i < items.length; i++)
            this.add(items[i][0], items[i][1]);
    }
};
Object.extend(Ajax.Web.NameValueCollection.prototype, {
    add: function (k, v) {
        if (k == null || k.constructor != String || v == null || v.constructor != String)
            return -1;
        this.keys.push(k);
        this.values.push(v);
        return this.values.length - 1;
    },
    containsKey: function (key) {
        for (var i = 0; i < this.keys.length; i++) {
            if (this.keys[i] == key) return true;
        }
        return false;
    },
    getKeys: function () {
        return this.keys;
    },
    getValue: function (k) {
        for (var i = 0; i < this.keys.length && i < this.values.length; i++) {
            if (this.keys[i] == k) return this.values[i];
        }
        return null;
    },
    setValue: function (k, v) {
        if (k == null || k.constructor != String || v == null || v.constructor != String)
            return -1;
        for (var i = 0; i < this.keys.length && i < this.values.length; i++) {
            if (this.keys[i] == k) this.values[i] = v;
            return i;
        }
        return this.add(k, v);
    },
    toJSON: function () {
        return AjaxPro.toJSON({ __type: this.__type, keys: this.keys, values: this.values });
    }
}, true);

// DataSetConverter
if (typeof Ajax == "undefined") Ajax = {};
if (typeof Ajax.Web == "undefined") Ajax.Web = {};
if (typeof Ajax.Web.DataSet == "undefined") Ajax.Web.DataSet = {};

Ajax.Web.DataSet = function (t) {
    this.__type = "System.Data.DataSet,System.Data";
    this.Tables = [];
    this.addTable = function (t) {
        this.Tables.push(t);
    };
    if (t != null) {
        for (var i = 0; i < t.length; i++) {
            this.addTable(t[i]);
        }
    }
};

// DataTableConverter
if (typeof Ajax == "undefined") Ajax = {};
if (typeof Ajax.Web == "undefined") Ajax.Web = {};
if (typeof Ajax.Web.DataTable == "undefined") Ajax.Web.DataTable = {};

Ajax.Web.DataTable = function (c, r) {
    this.__type = "System.Data.DataTable,System.Data";
    this.Columns = [];
    this.Rows = [];
    this.addColumn = function (name, type) {
        this.Columns.push({ Name: name, __type: type });
    };
    this.toJSON = function () {
        var dt = {};
        var i;
        dt.Columns = [];
        for (i = 0; i < this.Columns.length; i++)
            dt.Columns.push([this.Columns[i].Name, this.Columns[i].__type]);
        dt.Rows = [];
        for (i = 0; i < this.Rows.length; i++) {
            var row = [];
            for (var j = 0; j < this.Columns.length; j++)
                row.push(this.Rows[i][this.Columns[j].Name]);
            dt.Rows.push(row);
        }
        return AjaxPro.toJSON(dt);
    };
    this.addRow = function (row) {
        this.Rows.push(row);
    };
    if (c != null) {
        for (var i = 0; i < c.length; i++)
            this.addColumn(c[i][0], c[i][1]);
    }
    if (r != null) {
        for (var y = 0; y < r.length; y++) {
            var row = {};
            for (var z = 0; z < this.Columns.length && z < r[y].length; z++)
                row[this.Columns[z].Name] = r[y][z];
            this.addRow(row);
        }
    }
};

// ProfileBaseConverter
if (typeof Ajax == "undefined") Ajax = {};
if (typeof Ajax.Web == "undefined") Ajax.Web = {};
if (typeof Ajax.Web.Profile == "undefined") Ajax.Web.Profile = {};

Ajax.Web.Profile = function () {
    this.toJSON = function () {
        throw "Ajax.Web.Profile cannot be converted to JSON format.";
    };
    this.setProperty_callback = function (res) {
    };
    this.setProperty = function (name, object) {
        this[name] = object;
        AjaxPro.Services.Profile.SetProfile({ name: o }, this.setProperty_callback.bind(this));
    };
};

// IDictionaryConverter
if (typeof Ajax == "undefined") Ajax = {};
if (typeof Ajax.Web == "undefined") Ajax.Web = {};
if (typeof Ajax.Web.Dictionary == "undefined") Ajax.Web.Dictionary = {};

Ajax.Web.Dictionary = function (type, items) {
    this.__type = type;
    this.keys = [];
    this.values = [];

    if (items != null && !isNaN(items.length)) {
        for (var i = 0; i < items.length; i++)
            this.add(items[i][0], items[i][1]);
    }
};
Object.extend(Ajax.Web.Dictionary.prototype, {
    add: function (k, v) {
        this.keys.push(k);
        this.values.push(v);
        return this.values.length - 1;
    },
    containsKey: function (key) {
        for (var i = 0; i < this.keys.length; i++) {
            if (this.keys[i] == key) return true;
        }
        return false;
    },
    getKeys: function () {
        return this.keys;
    },
    getValue: function (key) {
        for (var i = 0; i < this.keys.length && i < this.values.length; i++) {
            if (this.keys[i] == key) { return this.values[i]; }
        }
        return null;
    },
    setValue: function (k, v) {
        for (var i = 0; i < this.keys.length && i < this.values.length; i++) {
            if (this.keys[i] == k) { this.values[i] = v; }
            return i;
        }
        return this.add(k, v);
    },
    toJSON: function () {
        return AjaxPro.toJSON({ __type: this.__type, keys: this.keys, values: this.values });
    }
}, true);

