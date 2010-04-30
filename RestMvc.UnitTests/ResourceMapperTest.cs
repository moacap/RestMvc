using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using RestMvc.Attributes;
using RestMvc.UnitTests.Assertions;

namespace RestMvc.UnitTests
{
    [TestFixture]
    public class ResourceMapperTest
    {
        public class EmptyController : RestfulController { }

        public class TestController : RestfulController
        {
            [Get("Test")]
            public void List() { }

            [Post("test")]
            public void Create() { }

            [Get("Test/{id}")]
            public void Show() { }
        }

        public class MultipleController : RestfulController
        {
            [Get("test1", "test2")]
            public void Test() { }
        }

        public class DifferentSuperclassController : Controller
        {
            [Get("test")]
            public void Test() { }
        }

        [Test]
        public void ControllerWithNoResourcesShouldNotMapsOptions()
        {
            var routes = new RouteCollection();
            var mapper = new ResourceMapper<EmptyController>(routes, new MvcRouteHandler());

            mapper.MapSupportedMethods();

            Assert.That(routes.Count, Is.EqualTo(0));
        }

        [Test]
        public void ShouldCreateRoutesForAnnotatedActions()
        {
            var routes = new RouteCollection();
            var mapper = new ResourceMapper<TestController>(routes, new MvcRouteHandler());

            mapper.MapSupportedMethods();

            Assert.That("GET /Test", Routes.To(new {controller = "Test", action = "List"}, routes));
            Assert.That("POST /Test", Routes.To(new {controller = "Test", action = "Create"}, routes));
        }

        [Test]
        public void ShouldRouteAllUrisInAttribute()
        {
            var routes = new RouteCollection();
            var mapper = new ResourceMapper<MultipleController>(routes, new MvcRouteHandler());

            mapper.MapSupportedMethods();

            Assert.That("GET /test1", Routes.To(new {controller = "Multiple", action = "Test"}, routes));
            Assert.That("GET /test2", Routes.To(new {controller = "Multiple", action = "Test"}, routes));
        }

        [Test]
        public void ShouldCreateMethodNotSupportedRoutesForUnmappedHttpMethods()
        {
            var routes = new RouteCollection();
            var mapper = new ResourceMapper<TestController>(routes, new MvcRouteHandler());

            mapper.MapUnsupportedMethods();

            var methodNotSupported = new {controller = "Test",
                action = RestfulController.MethodNotSupportedAction, resourceUri = "test"};
            Assert.That("DELETE /test", Routes.To(methodNotSupported, routes));
            Assert.That("PUT /test", Routes.To(methodNotSupported, routes));
        }

        [Test]
        public void ShouldCreateRestfulControllerForUnmappedHttpMethodsWithoutSubclassing()
        {
            var routes = new RouteCollection();
            var mapper = new ResourceMapper<DifferentSuperclassController>(routes, new MvcRouteHandler());

            mapper.MapUnsupportedMethods();

            var methodNotSupported = new {controller = "Restful",
                action = RestfulController.MethodNotSupportedAction, resourceUri = "test"};
            Assert.That("DELETE /test", Routes.To(methodNotSupported, routes));
        }

        [Test]
        public void ShouldMapHeadForAllResources()
        {
            var routes = new RouteCollection();
            var mapper = new ResourceMapper<TestController>(routes, new MvcRouteHandler());

            mapper.MapHead();

            Assert.That("HEAD /test", Routes.To(new {controller = "Test", action = "Head", resourceUri = "Test"}, routes));
            Assert.That("HEAD /test/1", Routes.To(new {controller = "Test", action = "Head", resourceUri = "Test/{id}"}, routes));
        }

        [Test]
        public void ShouldNotMapHeadWithoutSubclassing()
        {
            var routes = new RouteCollection();
            var mapper = new ResourceMapper<DifferentSuperclassController>(routes, new MvcRouteHandler());

            mapper.MapHead();

            Assert.That(routes.Count, Is.EqualTo(0));
        }

        [Test]
        public void ShouldMapOptionsForAllResources()
        {
            var routes = new RouteCollection();
            var mapper = new ResourceMapper<TestController>(routes, new MvcRouteHandler());

            mapper.MapOptions();

            Assert.That("OPTIONS /test", Routes.To(new {controller = "Test", action = "Options", resourceUri = "Test"}, routes));
            Assert.That("OPTIONS /test/1", Routes.To(new {controller = "Test", action = "Options", resourceUri = "Test/{id}"}, routes));
        }

        [Test]
        public void ShouldMapOptionsToDifferentControllerWithoutSubclassing()
        {
            var routes = new RouteCollection();
            var mapper = new ResourceMapper<DifferentSuperclassController>(routes, new MvcRouteHandler());

            mapper.MapOptions();

            Assert.That("OPTIONS /test", Routes.To(
                new {controller = "Restful", action = "Options", resourceUri = "Test", controllerType = typeof(DifferentSuperclassController)}, routes));
        }
    }
}
