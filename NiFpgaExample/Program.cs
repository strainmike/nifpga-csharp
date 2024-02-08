using NationalInstruments.NiFpga;
using System.Runtime.CompilerServices;

void PrintValue(dynamic value, string whitespace="")
{
    if (value is Dictionary<string, dynamic> dict)
    {
        foreach (var kvp in dict)
        {
            Console.WriteLine($"{whitespace}Name: {kvp.Key}");
            PrintValue(kvp.Value, whitespace + "   ");
        }
    }
    else if (value is Array array)
    {
        foreach (var item in array)
        {
            PrintValue(item, whitespace + "   ");
        }
    }
    else
    {
        Console.WriteLine($"{whitespace}Value: {value}");
    }
}

using (var session = new Session("cRIO-9068_allregistertypes.lvbitx", "rio://DRATS2-9068/RIO0"))
{
    session.reset();
    session.run();
    /*
    var register = session.Registers["Input U16"];
    int valueToWrite = 123;
    register.Write(valueToWrite);

    var readValue = register.Read();
    Console.WriteLine($"Read value Input U16: {readValue}");
    Console.WriteLine($"Read value Output U16: {session.Registers["Output U16"].Read()}");


    var registerBool = session.Registers["Input U64"];
    var registerBoolOut = session.Registers["Output U64"];
    registerBool.Write(123);
    Console.WriteLine($"Read value Output U64: {registerBoolOut.Read()}");

    var registerArray = session.Registers["Input Array U8"];
    Console.WriteLine($"Length value: {registerArray.Length}");
    var data = new byte[registerArray.Length];
    Console.WriteLine($"Read value Array U8: {string.Join(", ", registerArray.Read())}");
    data[0] = 1;
    data[1] = 1;
    data[2] = 3;
    data[3] = 4;
    data[4] = 5;
    data[5] = 6;
    registerArray.Write(data);
    Console.WriteLine($"Read value Input Array U8: {string.Join(", ", registerArray.Read())}");


    var registerArrayResult = session.Registers["Output Array U8"];
    Console.WriteLine($"Read value Output Array U8: {string.Join(", ", registerArrayResult.Read())}");



    var registerArray16 = session.Registers["Input Array U16"];
    Console.WriteLine($"Length value: {registerArray16.Length}");
    var data16 = new byte[registerArray16.Length];
    Console.WriteLine($"Read value Input Array U16: {string.Join(", ", registerArray16.Read())}");
    data16[0] = 1;
    data16[1] = 1;
    data16[2] = 3;
    data16[3] = 4;
    registerArray16.Write(data16);
    Console.WriteLine($"Read value Input Array U16: {string.Join(", ", registerArray16.Read())}");


    var registerArray16Result = session.Registers["Output Array U16"];
    Console.WriteLine($"Read value Output Array U16: {string.Join(", ", registerArray16Result.Read())}");

    var cluster = session.Registers["Input Small Cluster"];
    PrintValue(cluster.Read());
    var value = cluster.Read();
    Console.WriteLine($"Read value Cluster: {value}");
    Console.WriteLine($"Read value Cluster: {value["Bool Array"]}");
    value["Bool Array"][2] = true;
    cluster.Write(value);
    PrintValue(cluster.Read(), "    ");
    */

    
    var clusterComplex = session.Registers["Input Complex Cluster"];
    /*
    valueComplex["U32"] = 7;
    valueComplex["U16"] = 8;
    valueComplex["U8"] = 253;
    valueComplex["I32"] = 500;
    valueComplex["Cluster 1"]["I8"] = -12;
    valueComplex["Cluster 1"]["U8"] = 12;
    clusterComplex.Write(valueComplex);*/
    PrintValue(clusterComplex.Read());
    
}