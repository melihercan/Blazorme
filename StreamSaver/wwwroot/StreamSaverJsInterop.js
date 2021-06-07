StreamSaverJsInterop = (function () {

    let public = {};

    // Use same value on .NET side, "StreamSaverJsObjectRef.cs".
    const streamSaverJsObjectRefKey = '__streamSaverJsObjectRefId';

    let streamSaverObjectRefs = {};
    let streamSaverObjectRefId = 0;

    DotNet.attachReviver(function (key, value) {
        if (value && typeof value === 'object' && value.hasOwnProperty(streamSaverJsObjectRefKey) &&
            typeof value[streamSaverJsObjectRefKey] === 'number') {
            let id = value[streamSaverJsObjectRefKey];
            if (!(id in streamSaverObjectRefs)) {
                throw new Error('JS object reference with id:' + id + ' does not exist');
            }
            return streamSaverObjectRefs[id];
        } else {
            return value;
        }
    })

    getParentObject = function (parent) {
        let parentObject;
        if (typeof (parent) === 'string') {
            parentObject = getPropertyObject(window, parent);
        } else {
            parentObject = parent;
        }
        return parentObject;
    }

    addObjectRef = function (object) {
        let id = streamSaverObjectRefId++;
        streamSaverObjectRefs[id] = object;
        let objectRef = {};
        objectRef[streamSaverJsObjectRefKey] = id;
        return objectRef;
    }

    getPropertyObject = function (parentObject, property) {
        if (property === null) {
            return parentObject;
        }

        let list = property.replace('[', '.').replace(']', '').split('.');
        if (list[0] === "") {
            list.shift();
        }
        let object = parentObject;
        for (i = 0; i < list.length; i++) {
            if (list[i] in object) {
                object = object[list[i]];
            } else {
                throw new Error("Object referenced by " + property + " does not exist");
            }
        }
        return object;
    }

    getObjectContent = function (object, accumulatedContent, contentSpec) {
        if (contentSpec === false) {
            return undefined;
        }
        if (!accumulatedContent) {
            accumulatedContent = [];
        }
        if (typeof object === "undefined" || object === null) {
            return null;
        }
        if (typeof object === "number" || typeof object === "string" || typeof object === "boolean") {
            return object;
        }
        let content = (Array.isArray(object)) ? [] : {};
        if (!contentSpec) {
            contentSpec = "*";
        }
        for (let i in object) {
            let currentMember = object[i];
            if (typeof currentMember === 'function' || currentMember === null) {
                continue;
            }
            let currentMemberSpec;
            if (contentSpec !== "*") {
                currentMemberSpec = Array.isArray(object) ? contentSpec : contentSpec[i];
                if (!currentMemberSpec) {
                    continue;
                }
            } else {
                currentMemberSpec = "*"
            }
            if (typeof currentMember === 'object') {
                if (accumulatedContent.indexOf(currentMember) >= 0) {
                    continue;
                }
                accumulatedContent.push(currentMember);
                if (Array.isArray(currentMember) || currentMember.length) {
                    content[i] = [];
                    for (let j = 0; j < currentMember.length; j++) {
                        const arrayItem = currentMember[j];
                        if (typeof arrayItem === 'object') {
                            content[i].push(getObjectContent(arrayItem, accumulatedContent, currentMemberSpec));
                        } else {
                            content[i].push(arrayItem);
                        }
                    }
                } else {
                    if (currentMember.length === 0) {
                        content[i] = [];
                    } else {
                        content[i] = getObjectContent(currentMember, accumulatedContent, currentMemberSpec);
                    }
                }
            } else {
                if (currentMember === Infinity) {
                    currentMember = "Infinity";
                }
                if (currentMember !== null) {
                    content[i] = currentMember;
                }
            }
        }
        return content;
    };

    /*
     *
     * public API
     *
     */

    /**
     * Create a new object, add the object to 'objectRefs' and return JS object reference.
     * 
     * @param {any} parent: Parent object. It can be JS object reference or a string. JS object reference will be
     *                      converted into a JS object by the reviver. If it is a string, it will be converted into
     *                      JS object first.
     * @param {string} interface: Interface(class) name to be created.
     * @param {...any} args: Argument list of the constructor.
     */
    public.createObject = function (parent, interface, ...args) {
        let parentObject = getParentObject(parent);;
        let interfaceObject = getPropertyObject(parentObject, interface);
        let createdObject = new interfaceObject(args);
        let objectRef = addObjectRef(createdObject);
        return objectRef;
    }

    /**
     * Delete the specified JS object reference from 'objectRefs'.
     * 
     * @param {number} id
     */
    public.deleteObjectRef = function (id) {
        delete streamSaverObjectRefs[id];
    }

    /**
     * Gets object reference of the specified property. 
     * 
     * @param {any} parent: Parent object. It can be JS object reference or a string. JS object reference will be 
     *                      converted into a JS object by the reviver. If it is a string, it will be converted into
     *                      JS object first.
     * @param {string} property: String specifying the property to get. If 'null', parent object will be returned.
     */
    public.getPropertyObjectRef = function (parent, property) {
        let parentObject = getParentObject(parent);

        let propertyObject = getPropertyObject(parentObject, property);
        if (typeof (propertyObject) === 'object' && propertyObject !== null) {
            let objectRef = addObjectRef(propertyObject);
            return objectRef;
        } else {
            return null;
        }
    }

    /**
     * Gets object as value. If 'valueSpec' is provided then the values specified in 'valueSpec' will be returned.
     * Otherwise it will be just a normal return. 
     * 
     * @param {any} parent: Parent object. It can be JS object reference or a string. JS object reference will be 
     *                      converted into a JS object by the reviver. If it is a string, it will be converted into
     *                      JS object first.
     * @param {string} property: String specifying the property to get. If 'null', parent object will be returned.
     * @param {string} contentSpec: Filter of the content to be returned. 'null' indicates that JS object reference
     *                              shall be returned if property specifies an 'object'
     */
    public.getPropertyValue = function (parent, property, contentSpec) {
        let parentObject = getParentObject(parent);

        let propertyObject = getPropertyObject(parentObject, property);
        if (typeof (propertyObject) === 'object' && contentSpec !== null) {
            let value = getObjectContent(propertyObject, [], contentSpec);
            return value;
        } else {
            return propertyObject;
        }
    }

    /**
     * Gets the array of object references.
     * 
     * @param {any} parent: Parent object. It can be JS object reference or a string. JS object reference will be
     *                      converted into a JS object by the reviver. If it is a string, it will be converted into
     *                      JS object first.
     * @param {string} property: String specifying the property to get. If 'null', parent object will be returned.
     */
    public.getPropertyArray = function (parent, property) {
        let parentObject = getParentObject(parent);
        let arrayObject = getPropertyObject(parentObject, property);
        if (Array.isArray(arrayObject)) {
            let objectRefArray = [];
            arrayObject.forEach(object => {
                let objectRef = addObjectRef(object);
                objectRefArray.push(objectRef);
            });
            return objectRefArray;
        } else {
            throw new Error("Object does not contain array");
        }
    } 


    /**
     * Sets specified property.
     * 
     * @param {any} parent: Parent object. It can be JS object reference or a string. JS object reference will be
     *                      converted into a JS object by the reviver. If it is a string, it will be converted into
     *                      JS object first.
     * @param {string} property: String specifying the property to set. If 'null', parent object will be set.
     * @param {any} value: Object to set. It can be JS object reference or a string. JS object reference will be
     *                     converted into a JS object by the reviver. If it is a string, it will be converted into
     *                     JS object first.
     */
    public.setProperty = function (parent, property, value) {
        let parentObject = getParentObject(parent);
    //    let valueObject;
    //    if (typeof (value) === 'string') {
    //        valueObject = getPropertyObject(window, value);
    //    } else {
    //        valueObject = value;
    //    }
    //    parentObject[property] = valueObject;
        parentObject[property] = value;

    }

    /**
     * Calls a method synchronously. If return value is object type, it adds the object to 'objectRefs' and 
     * returns JS object reference. Otherwise the primitive type is returned.
     * 
     * @param {any} parent: Parent object. It can be JS object reference or a string. JS object reference will be
     *                      converted into a JS object by the reviver. If it is a string, it will be converted into
     *                      JS object first.
     * @param {string} method: String specifying the method to be called.
     * @param {...any} args: Argument list of the method.
     */
    public.callMethod = function (parent, method, ...args) {
        let parentObject = getParentObject(parent);
        let methodObject = getPropertyObject(parentObject, method);
        let ret = methodObject.apply(parentObject, args);
        if (ret !== undefined) {
            if (ret !== null && typeof (ret) === 'object') {
                let objectRef = addObjectRef(ret);
                return objectRef;
            } else {
                return ret;
            }
        }
    }

    /**
     * Calls a method asynchronously. it waits for the promise to be completed. If return value is object type, 
     * it adds the object to 'objectRefs' and returna JS object reference. Otherwise the primitive type is returned.
     *
     * @param {any} parent: Parent object. It can be JS object reference or a string. JS object reference will be
     *                      converted into a JS object by the reviver. If it is a string, it will be converted into
     *                      JS object first.
     * @param {string} method: String specifying the method to be called.
     * @param {...any} args: Argument list of the method.
     */
    public.callMethodAsync = async function (parent, method, ...args) {
        let parentObject = getParentObject(parent);
        let methodObject = getPropertyObject(parentObject, method);
        let ret = await methodObject.apply(parentObject, args);
        if (ret !== undefined) {
            if (ret !== null && typeof (ret) === 'object') {
                let objectRef = addObjectRef(ret);
                return objectRef;
            } else {
                return ret;
            }
        }
    }

    return public;

})();