﻿using MercadoPago;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MercadoPagoSDK.Test
{
    [TestFixture()]
    public class MPBaseTest : MPBase
    {    
        public static MPBaseTest Load(string id)
        {
            return (MPBaseTest)ProcessMethod("Load", id, false);
        }

        [GETEndpoint("/v1/getpath/slug")]
        public static MPBaseTest Load_all()
        {
            return (MPBaseTest)ProcessMethod("Load_all", false);
        }

        [Test()]
        public void MPBaseTest_WithNoAttributes_ShouldraiseException()
        {
            try
            {
                var result = Load("666");
            }
            catch (MPException mpException)
            {
                Assert.AreEqual("No annotated method found", mpException.Message);
                return;
            }

            // should never get here
            Assert.Fail();
        }
     
        [Test()]
        public void MPBaseTest_WitAttributes_ShouldFindAttribute()
        {
            MPConf.CleanConfiguration();
            MPConf.SetBaseUrl("https://api.mercadopago.com");

            try
            {
                var result = Load_all();
            }
            catch (MPException mpException)
            {
                // should never get here
                Assert.Fail();
                return;
            }

            Assert.Pass();
        }
    }

    [Idempotent]
    [TestFixture()]    
    public class DummyClass : MPBase
    {
        public int id { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string maritalStatus { get; set; }
        public bool hasCreditCard { get; set; }

        public static DummyClass Load_all()
        {
            return (DummyClass)ProcessMethod("Load_all", false);
        }

        [GETEndpoint("/v1/getpath/load/:id")]
        public static DummyClass Load(string id)
        {
            return (DummyClass)ProcessMethod("Load", id, false);
        }

        [POSTEndpoint("/v1/postpath/slug")]
        public DummyClass Create()
        {
            return (DummyClass)ProcessMethod<DummyClass>("Create", false);
        }


        [PUTEndpoint("/v1/putpath/slug")]
        public DummyClass Update()
        {
            return (DummyClass)ProcessMethod("Update", false);
        }

        [Test()]
        public void DummyClassMethod_WithNoAttributes_ShouldraiseException()
        {
            try
            {
                var result = Load_all();
            }
            catch (MPException mpException)
            {
                Assert.AreEqual("No annotated method found", mpException.Message);
                return;
            }

            // should never get here
            Assert.Fail();
        }      

        [Test()]
        public void DummyClassMethod_WitAttributes_ShouldFindAttribute()
        {
            MPConf.CleanConfiguration();
            MPConf.SetBaseUrl("https://api.mercadopago.com");

            try
            {
                var result = Load("1234");
            }
            catch 
            {
                // should never get here
                Assert.Fail();
                return;
            }

            Assert.Pass();
        }

        [Test()]
        public void DummyClassMethod_Create_CheckUri()
        {
            MPConf.CleanConfiguration();
            MPConf.SetBaseUrl("https://api.mercadopago.com");

            DummyClass resource = new DummyClass();
            resource.address = "Evergreen 123";
            resource.email = "fake@email.com";

            DummyClass result = new DummyClass();
            try
            {
                result = resource.Create();
            }
            catch
            {
                // should never get here
                Assert.Fail();
                return;
            }

            Assert.AreEqual("POST", result.LastApiResponse.HttpMethod);
            Assert.AreEqual("https://api.mercadopago.com/v1/postpath/slug", result.LastApiResponse.Url);
        }

        [Test()]
        public void DummyClassMethod_Update_CheckUri()
        {
            MPConf.CleanConfiguration();
            MPConf.SetBaseUrl("https://api.mercadopago.com");

            DummyClass resource = new DummyClass();
            resource.address = "Evergreen 123";
            resource.email = "fake@email.com";

            DummyClass result = new DummyClass();
            try
            {
                result = resource.Update();
            }
            catch
            {
                // should never get here
                Assert.Fail();
                return;
            }

            Assert.AreEqual("PUT", result.LastApiResponse.HttpMethod);
            Assert.AreEqual("https://api.mercadopago.com/v1/putpath/slug", result.LastApiResponse.Url);
        }

        [Test()]
        public void DummyClassMethod_WithoutClassReference()
        {
            try
            {
                var result = Load_all();
            }
            catch (MPException mpException)
            {
                Assert.AreEqual("No annotated method found", mpException.Message);
                return;
            }

            // should never get here
            Assert.Fail();
        }
    }

    [TestFixture()]
    public class AnotherDummyClass : MPBase
    {
        [PUTEndpoint("")]
        public AnotherDummyClass Update()
        {
            return (AnotherDummyClass)ProcessMethod("Update", false);
        }

        [Test()]
        public void AnotherDummyClass_EmptyEndPoint_ShouldRaiseExcep()
        {
            AnotherDummyClass resource = new AnotherDummyClass();
            AnotherDummyClass result = new AnotherDummyClass();
            try
            {
                result = resource.Update();
            }
            catch (MPException ex)
            {
                Assert.AreEqual("Path not found for PUT method", ex.Message);
                return;
            }

            Assert.Fail();
        }

        [Test()]
        public void MPBase_ParsePath_ShouldReplaceParamInUrlWithValues()
        {
            DummyClass dummy = new DummyClass();
            dummy.id = 111;
            dummy.email = "person@something.com";
            dummy.address = "Evergreen 123";
            dummy.maritalStatus = "divorced";
            dummy.hasCreditCard = true;

            try
            {
                string processedPath = ParsePath("https://api.mercadopago.com/v1/getpath/slug/:id/pUnexist/:unexist", null, dummy);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("No argument supplied/found for method path", ex.Message);
            }

            string processedPath0 = ParsePath("/v1/getpath/slug", null, dummy);
            Assert.AreEqual("https://api.mercadopago.com/v1/getpath/slug", processedPath0);

            string processedPath1 = ParsePath("/v1/putpath/slug/:id/pEmail/:email", null, dummy);
            Assert.AreEqual("https://api.mercadopago.com/v1/putpath/slug/111/pEmail/person@something.com", processedPath1);

            string processedPath2 = ParsePath("/v1/putpath/slug/:id/pHasCreditCard/:hasCreditCard", null, dummy);
            Assert.AreEqual("https://api.mercadopago.com/v1/putpath/slug/111/pHasCreditCard/True", processedPath2);

            string processedPath3 = ParsePath("/v1/putpath/slug/:id/pEmail/:email/pAddress/:address", null, dummy);
            Assert.AreEqual("https://api.mercadopago.com/v1/putpath/slug/111/pEmail/person@something.com/pAddress/Evergreen 123", processedPath3);

            string processedPath4 = ParsePath("/v1/putpath/slug/:id/pEmail/:email/pAddress/:address/pMaritalstatus/:maritalStatus/pHasCreditCard/:hasCreditCard", null, dummy);
            Assert.AreEqual("https://api.mercadopago.com/v1/putpath/slug/111/pEmail/person@something.com/pAddress/Evergreen 123/pMaritalstatus/divorced/pHasCreditCard/True", processedPath4);

        }
    }

    [Idempotent]
    [TestFixture()]
    public class CustomerTestClass : MPBase
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }

        [POSTEndpoint("/post")]
        public CustomerTestClass Create()
        {
            return (CustomerTestClass)ProcessMethod<CustomerTestClass>("Create", false);
        }


        [Test()]
        public void CustomerTestClass_Create_ParsesCustomerTestClassObjectResponse()
        {
            MPConf.CleanConfiguration();
            MPConf.SetBaseUrl("https://httpbin.org");

            CustomerTestClass resource = new CustomerTestClass();
            resource.Name = "Bruce";
            resource.LastName = "Wayne";
            resource.Age = 45;

            CustomerTestClass result = new CustomerTestClass();
            try
            {
                result = resource.Create();
            }
            catch
            {
                // should never get here
                Assert.Fail();
                return;
            }

            Assert.AreEqual("POST", result.LastApiResponse.HttpMethod);
            Assert.AreEqual("https://httpbin.org/post", result.LastApiResponse.Url);
            Assert.AreEqual("Bruce", result.Name);
            Assert.AreEqual("Wayne", result.LastName);
            Assert.AreEqual(45, result.Age);
        }

        [Test()]
        public void CustomerTestClass_Create_CheckFullJsonResponse()
        {
            MPConf.CleanConfiguration();
            MPConf.SetBaseUrl("https://httpbin.org");

            CustomerTestClass resource = new CustomerTestClass();
            resource.Name = "Bruce";
            resource.LastName = "Wayne";
            resource.Age = 45;

            CustomerTestClass result = new CustomerTestClass();
            try
            {
                result = resource.Create();
            }
            catch
            {
                // should never get here
                Assert.Fail();
                return;
            }

            Assert.AreEqual("POST", result.LastApiResponse.HttpMethod);
            Assert.AreEqual("https://httpbin.org/post", result.LastApiResponse.Url);

            JObject jsonResponse = result.GetJsonSource();
            List<JToken> lastName = MPCoreUtils.FindTokens(jsonResponse, "LastName");
            Assert.AreEqual("Wayne", lastName.First().ToString());

            List<JToken> year = MPCoreUtils.FindTokens(jsonResponse, "Name");
            Assert.AreEqual("Bruce", year.First().ToString());
        }
    }  
}