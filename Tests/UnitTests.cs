using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Woof.WindowsEx.Notifications;

[TestClass]
public class UnitTests {

    [TestMethod]
    public void NotifyTest() {
        var n = new Notification("Unit Test");
        n.Notify(Notification.Contexts.Message, "Hello world!");
        
    }

}