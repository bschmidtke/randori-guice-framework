/***
 * Copyright 2012 LTN Consulting, Inc. /dba Digital Primates�
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * @author Michael Labriola <labriola@digitalprimates.net>
 */

using System;
using SharpKit.Html;
using SharpKit.JavaScript;
using guice.binding;
using guice.reflection;
using guice.resolvers;

namespace guice {

    [JsType(JsMode.Prototype, NativeOverloads = false)]
    public class Injector {
        readonly Binder binder;
        readonly ClassResolver classResolver;

        public object getInstance( Type dependency ) {
            return resolveDependency( new TypeDefinition(dependency) );
        }

        public object getInstance(TypeDefinition dependencyTypeDefinition) {
            return resolveDependency(dependencyTypeDefinition);
        }

        //Entry point for TypeBinding to ask for a class.... 
        //This method does so without trying to resolve the class first, which is important if we are called from within a resolution
        public object buildClass(Type dependency) {
            TypeDefinition type = dependency.As<TypeDefinition>();
            JsArray<InjectionPoint> constructorPoints;
            JsArray<InjectionPoint> fieldPoints;
            object instance;

            constructorPoints = type.getConstructorParameters();
            instance = buildFromInjectionInfo(type, constructorPoints);

            fieldPoints = type.getInjectionFields();
            injectMembersFromInjectionInfo(instance, fieldPoints);
            //injectMembersMethods( built, type );

            return instance;
        }

        public void injectMembers(dynamic instance) {
            Type constructor = instance.constructor;

            TypeDefinition dependency = new TypeDefinition( constructor );
            JsArray<InjectionPoint> fieldPoints;

            fieldPoints = dependency.getInjectionFields();
            injectMembersFromInjectionInfo(instance, fieldPoints);
        }

        object buildFromInjectionInfo(TypeDefinition dependency, JsArray<InjectionPoint> constructorPoints) {
            JsArray<object> args = new JsArray<object>();

            for (int i = 0; i < constructorPoints.length; i++) {
                args[i] = resolveDependency(classResolver.resolveClassName(constructorPoints[i].t));
            }

            object obj = dependency.constructorApply(args);
            return obj;
        }

        void injectMembersFromInjectionInfo(object instance, JsArray<InjectionPoint> fieldPoints) {
            JsObject instanceMap = instance.As<JsObject>();

            for (int i = 0; i < fieldPoints.length; i++) {
                instanceMap[fieldPoints[i].n] = resolveDependency(classResolver.resolveClassName(fieldPoints[i].t));
            }
        }

        object resolveDependency(TypeDefinition dependency) {
            Binding binding = binder.getBinding(dependency);
            object instance;

            if (binding != null) {
                instance = binding.provide(this);
            } else {
                instance = buildClass(dependency.As<Type>());
            }

            return instance;
        }

        public Injector(Binder binder, ClassResolver classResolver) {
            this.binder = binder;
            this.classResolver = classResolver;
        }
    }
}