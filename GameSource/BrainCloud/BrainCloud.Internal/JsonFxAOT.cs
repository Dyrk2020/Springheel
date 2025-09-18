using System.ComponentModel;

namespace BrainCloud.Internal;

internal class JsonFxAOT
{
	private static JsonFxAOT m_aot = new JsonFxAOT();

	private bool m_fakeFlag;

	private JsonFxAOT()
	{
		TypeConverter typeConverter = new ArrayConverter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new BooleanConverter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new ByteConverter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new CollectionConverter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new ComponentConverter(typeof(int));
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new CultureInfoConverter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new DateTimeConverter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new DecimalConverter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new DoubleConverter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new EnumConverter(typeof(int));
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new ExpandableObjectConverter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new Int16Converter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new Int32Converter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new Int64Converter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new NullableConverter(typeof(object));
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new SByteConverter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new SingleConverter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new StringConverter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new TimeSpanConverter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new UInt16Converter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new UInt32Converter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
		typeConverter = new UInt64Converter();
		m_fakeFlag = typeConverter.Equals(typeConverter);
	}

	private bool GetFakeFlag()
	{
		return m_fakeFlag;
	}

	private JsonFxAOT GetAOT()
	{
		return m_aot;
	}
}
