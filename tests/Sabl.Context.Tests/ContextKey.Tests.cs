// Copyright 2023 Joshua Honig. All rights reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.

using System.Text;

namespace Sabl.ContextKeyClass;

public class Ctor
{
    [Fact]
    public void InitsFields()
    {
        var key = new ContextKey<int>("number");
        Assert.Equal("number", key.Label);
        Assert.False(key.Nullable);
        Assert.Equal(typeof(int), key.ValueType);
    }

    [Fact]
    public void DefaultsLabelToTypeName()
    {
        var key = new ContextKey<string>();
        Assert.Equal("String", key.Label);
    }

    [Fact]
    public void IdentifiesNonNullStruct()
    {
        var key = new ContextKey<int>();
        Assert.False(key.Nullable);
    }

    [Fact]
    public void IdentifiesNullableStruct()
    {
        var key = new ContextKey<int?>();
        Assert.True(key.Nullable);
    }

    [Fact]
    public void IdentifiesReferenceType()
    {
        var key = new ContextKey<Encoding>();
        Assert.True(key.Nullable);
    }
}

public class GetHashCode
{
    [Fact]
    public void ReturnsCachedStringHash()
    {
        var s = "hello";
        var hash = s.GetHashCode();
        var k = new ContextKey<object>(s);
        Assert.Equal(hash, k.GetHashCode());
    }
}

public class Equals
{
    [Fact]
    public void UsesReferenceEquality()
    {
        var k1 = new ContextKey<int>();
        var k2 = new ContextKey<int>();

        Assert.Equal(k1.Label, k2.Label);
        Assert.False(k1.Equals(k2));
        Assert.True(k1.Equals(k1));
    }
}

public class EqualsOp
{
    [Fact]
    public void UsesReferenceEquality()
    {
        var k1 = new ContextKey<int>();
        var k2 = new ContextKey<int>();

        Assert.Equal(k1.Label, k2.Label);
        Assert.False(k1 == k2);
#pragma warning disable CS1718 // Comparison made to same variable
        Assert.True(k1 == k1);
#pragma warning restore CS1718 // Comparison made to same variable
    }
}

public class NotEqualsOp
{
    [Fact]
    public void UsesReferenceEquality()
    {
        var k1 = new ContextKey<int>();
        var k2 = new ContextKey<int>();

        Assert.Equal(k1.Label, k2.Label);
        Assert.True(k1 != k2);
#pragma warning disable CS1718 // Comparison made to same variable
        Assert.False(k1 != k1);
#pragma warning restore CS1718 // Comparison made to same variable
    }
}

public class ToString
{
    [Fact]
    public void WrapsLabel()
    {
        var k = new ContextKey<int>("number");
        var s = k.ToString();
        Assert.Equal("Key(number)", s);
    }

    [Fact]
    public void InterpolatedString()
    {
        var k = new ContextKey<int>("number");
        var s = $"The value is {k}";
        Assert.Equal("The value is Key(number)", s);
    }
}
