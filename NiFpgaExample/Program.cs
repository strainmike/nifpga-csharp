using NationalInstruments.NiFpga;

Console.WriteLine("Hello, World!");

using (var session = new Session("cRIO-9068_allregistertypes.lvbitx", "rio://DRATS2-9068/RIO0"))
{
    if (session.Registers.TryGetValue("Input U32", out var register))
    {
        // Assuming the register is of type int for this example
        int valueToWrite = 123;
        register.Write(valueToWrite);

        var readValue = register.Read();
        Console.WriteLine($"Read value: {readValue}");
    }
    else
    {
        Console.WriteLine("Register not found");
    }
}