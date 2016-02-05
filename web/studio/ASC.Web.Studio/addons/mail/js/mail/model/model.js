Model = function(attributes, options) {
    var attrs = attributes || {};
    options || (options = {});
    this.attributes = {};
    this.set(attrs, options);
    this.initialize.apply(this, arguments);
};

jq.extend(Model.prototype, {
    idAttribute: 'id',

    initialize: function () { },

    srvSave: function () { },

    srvDelete: function () { },

    fetch: function() { },

    get: function (attr) {
        return this.attributes[attr];
    },

    escape: function (attr) {
        return jq.escape(this.get(attr));
    },

    has: function (attr) {
        return this.get(attr) != null;
    },

    clone: function () {
        return new this.constructor(this.attributes);
    },

    set: function (key, val, options) {
        if (key == null) return this;

        // Handle both `"key", value` and `{key: value}` -style arguments.
        var attrs;
        if (typeof key === 'object') {
            attrs = key;
            options = val;
        } else {
            (attrs = {})[key] = val;
        }

        options || (options = {});

        // Extract attributes and options.
        var unset = options.unset;
        var silent = options.silent;
        var changes = [];

        var current = this.attributes;

       // For each `set` attribute, update or delete the current value.
        for (var attr in attrs) {
            val = attrs[attr];
            if (current[attr] != val) changes.push(attr);
            unset ? delete current[attr] : current[attr] = val;
        }

        // Update the `id`.
        this.id = this.get(this.idAttribute);

        // Trigger all relevant attribute changes.
        if (!silent) {
            if (changes.length) this._pending = options;
            for (var i = 0; i < changes.length; i++) {
                jq(this).trigger('change:' + changes[i], this, current[changes[i]], options);
            }
        }

        return this;
    },

    // Remove an attribute from the model, firing `"change"`.
    unset: function (attr, options) {
        return this.set(attr, void 0, jq.extend({}, options, { unset: true }));
    },

    clear: function (options) {
        var attrs = {};
        for (var key in this.attributes) attrs[key] = void 0;
        return this.set(attrs, jq.extend({}, options, { unset: true }));
    },
});