#Problems with CultureInfo, ThreadPool and ExecutionContext


##Use case
I have in my application resources for pl-PL and en-US culture. Based on call context, WCF server should be able to use correct version of resources.


##Symptoms
On some service calls values extracted from resources were incorrect. Despite of setting context to pl-PL resources that were used were en-US.


##Environment
I have Windows 10 Pro English Language (originally problem noticed on Windows 7 Enterprise and Windows server 2008 R2), regional settings are PL.

Output of `CultureInfoAndThreads.exe ShowFrameworkVersionInfo`
```
Version info:
File:             C:\Windows\Microsoft.NET\Framework64\v4.0.30319\mscorlib.dll
InternalName:     mscorlib.dll
OriginalFilename: mscorlib.dll
FileVersion:      4.6.1073.0 built by: NETFXREL3STAGE
FileDescription:  Microsoft Common Language Runtime Class Library
Product:          MicrosoftR .NET Framework
ProductVersion:   4.6.1073.0
Debug:            False
Patched:          False
PreRelease:       False
PrivateBuild:     True
SpecialBuild:     False
Language:         English (United States)

Registry version: 4.6 or later

Environment version: 4.0.30319.42000

Updates:
Microsoft .NET Framework 4 Client Profile
  KB2468871
  KB2468871v2
  KB2478063
  KB2533523
  KB2544514
  KB2600211
  KB2600217
Microsoft .NET Framework 4 Extended
  KB2468871
  KB2468871v2
  KB2478063
  KB2533523
  KB2544514
  KB2600211
  KB2600217
Microsoft .NET Framework 4 Multi-Targeting Pack
  KB2504637  Update for  (KB2504637)
```

##Setup


start two consoles

in first enter `CultureInfoAndThreads.exe StartServer`
in second enter `CultureInfoAndThreads.exe StartClientCaseX` few times.

where X is 1, 2, 3 or 4.

##Testing

Every case was tested with newly started server.

In my test application I have this following code in IParameterInspector.BeforeCall in each test case

###Case1

code
```
            var cultureInfo = new CultureInfo(TestCulture);
            var thread = Thread.CurrentThread;

            thread.CurrentCulture = cultureInfo;
            thread.CurrentUICulture = cultureInfo;

```
server log
```
ManagedThreadId: 1 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
Server started. Press Ctrl+C to quit.
ManagedThreadId: 3 CurrentUICulture: pl-PL CurrentCulture: pl-PL TestKey resource: Wartoœæ testowa
Waiting for requests... Press Ctrl+C to quit.
ManagedThreadId: 3 CurrentUICulture: pl-PL CurrentCulture: pl-PL TestKey resource: Wartoœæ testowa
ManagedThreadId: 8 CurrentUICulture: pl-PL CurrentCulture: pl-PL TestKey resource: Wartoœæ testowa
ManagedThreadId: 8 CurrentUICulture: pl-PL CurrentCulture: pl-PL TestKey resource: Wartoœæ testowa
ManagedThreadId: 8 CurrentUICulture: pl-PL CurrentCulture: pl-PL TestKey resource: Wartoœæ testowa
ManagedThreadId: 8 CurrentUICulture: pl-PL CurrentCulture: pl-PL TestKey resource: Wartoœæ testowa
ManagedThreadId: 3 CurrentUICulture: pl-PL CurrentCulture: pl-PL TestKey resource: Wartoœæ testowa
ManagedThreadId: 3 CurrentUICulture: pl-PL CurrentCulture: pl-PL TestKey resource: Wartoœæ testowa
ManagedThreadId: 8 CurrentUICulture: pl-PL CurrentCulture: pl-PL TestKey resource: Wartoœæ testowa
Waiting for requests... Press Ctrl+C to quit.
Server stopped.

```

###Case2

code
```
            var cultureInfo = CultureInfo.GetCultureInfo(TestCulture);
            var thread = Thread.CurrentThread;

            thread.CurrentCulture = cultureInfo;
            thread.CurrentUICulture = cultureInfo;
```

server log

```
ManagedThreadId: 1 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
Server started. Press Ctrl+C to quit.
ManagedThreadId: 4 CurrentUICulture: pl-PL CurrentCulture: pl-PL TestKey resource: Wartoœæ testowa
ManagedThreadId: 6 CurrentUICulture: pl-PL CurrentCulture: pl-PL TestKey resource: Wartoœæ testowa
ManagedThreadId: 6 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
ManagedThreadId: 6 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
ManagedThreadId: 6 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
ManagedThreadId: 6 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
ManagedThreadId: 7 CurrentUICulture: pl-PL CurrentCulture: pl-PL TestKey resource: Wartoœæ testowa
Waiting for requests... Press Ctrl+C to quit.
ManagedThreadId: 6 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
ManagedThreadId: 6 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
ManagedThreadId: 6 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
ManagedThreadId: 6 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
ManagedThreadId: 6 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
ManagedThreadId: 6 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
ManagedThreadId: 4 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
ManagedThreadId: 4 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
Waiting for requests... Press Ctrl+C to quit.
ManagedThreadId: 6 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
ManagedThreadId: 4 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
ManagedThreadId: 4 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
ManagedThreadId: 4 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
ManagedThreadId: 6 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
Waiting for requests... Press Ctrl+C to quit.
ManagedThreadId: 4 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
ManagedThreadId: 6 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
ManagedThreadId: 6 CurrentUICulture: en-US CurrentCulture: pl-PL TestKey resource: Test Value
Waiting for requests... Press Ctrl+C to quit.
Server stopped.
```

Case3 and Case4 behave like Case1 and Case2.




##Observations
ManagedThreadId 1 is the main console application thread. It has default settings.


###Case1

Everything works fine, threads 3 and 8 have correct UI culture for all requests.

###Case2

**Houston we have a problem**, notice that every time WCF server is taking a new thread form a thread pool to process request, resources are displayed correctly.
When thread is reused in a following request, resources are incorrect and CurrentUICulture is incorrect. Only first time threads 4 and 6 displayed correct resource text in Case2.


##Investigation


So why is that we have all of this.

when we debug my application, we notice that ExecutionContext in subsequent requests on the same thread is present and it has values from previous request. **WAT!!??!!**

when we break just before setting CurrentUICulture and add to quick watch this `ExecutionContext.GetLocalValue(Thread.s_asyncLocalCurrentUICulture)` we can see that is already has "pl-PL", for the new thread is has null, remember this line.

with this knowledge we can check what happens when we set our CurrentUICulture 

Thread class
```
public CultureInfo CurrentUICulture
{
	[__DynamicallyInvokable]
	get
	{
		if (AppDomain.IsAppXModel())
		{
			return CultureInfo.GetCultureInfoForUserPreferredLanguageInAppX() ?? this.GetCurrentUICultureNoAppX();
		}
		return this.GetCurrentUICultureNoAppX();
	}
	[__DynamicallyInvokable, SecuritySafeCritical]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	set
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		CultureInfo.VerifyCultureName(value, true);
		if (!Thread.nativeSetThreadUILocale(value.SortName))
		{
			throw new ArgumentException(Environment.GetResourceString("Argument_InvalidResourceCultureName", new object[]
			{
				value.Name
			}));
		}
		value.StartCrossDomainTracking();
		if (!AppContextSwitches.NoAsyncCurrentCulture)
		{
			if (Thread.s_asyncLocalCurrentUICulture == null)
			{
				Interlocked.CompareExchange<AsyncLocal<CultureInfo>>(ref Thread.s_asyncLocalCurrentUICulture, new AsyncLocal<CultureInfo>(new Action<AsyncLocalValueChangedArgs<CultureInfo>>(Thread.AsyncLocalSetCurrentUICulture)), null);
			}
			Thread.s_asyncLocalCurrentUICulture.Value = value;
			return;
		}
		this.m_CurrentUICulture = value;
	}
}


``` 

lets look at line `Thread.s_asyncLocalCurrentUICulture.Value = value;` 

digging deeper...

AsyncLocal of T class
```
public T Value
{
	[__DynamicallyInvokable, SecuritySafeCritical]
	get
	{
		object localValue = ExecutionContext.GetLocalValue(this);
		if (localValue != null)
		{
			return (T)((object)localValue);
		}
		return default(T);
	}
	[__DynamicallyInvokable, SecuritySafeCritical]
	set
	{
		ExecutionContext.SetLocalValue(this, value, this.m_valueChangedHandler != null);
	}
}
```
do you recognize this line `ExecutionContext.GetLocalValue(this);`?
we have to dig deeper... 


ExecutionContext class

```
internal static void SetLocalValue(IAsyncLocal local, object newValue, bool needChangeNotifications)
{
	ExecutionContext mutableExecutionContext = Thread.CurrentThread.GetMutableExecutionContext();
	object obj = null;
	bool flag = mutableExecutionContext._localValues != null && mutableExecutionContext._localValues.TryGetValue(local, out obj);
	if (obj == newValue)
	{
		return;
	}
	if (mutableExecutionContext._localValues == null)
	{
		mutableExecutionContext._localValues = new Dictionary<IAsyncLocal, object>();
	}
	else
	{
		ExecutionContext expr_42 = mutableExecutionContext;
		expr_42._localValues = new Dictionary<IAsyncLocal, object>(expr_42._localValues);
	}
	mutableExecutionContext._localValues[local] = newValue;
	if (needChangeNotifications)
	{
		if (!flag)
		{
			if (mutableExecutionContext._localChangeNotifications == null)
			{
				mutableExecutionContext._localChangeNotifications = new List<IAsyncLocal>();
			}
			else
			{
				ExecutionContext expr_7B = mutableExecutionContext;
				expr_7B._localChangeNotifications = new List<IAsyncLocal>(expr_7B._localChangeNotifications);
			}
			mutableExecutionContext._localChangeNotifications.Add(local);
		}
		local.OnValueChanged(obj, newValue, false);
	}
}

```

GetLocalValue and SetLocalValue are using `ExecutionContext._localValues` to extract our culture.

Lets look at line `if (obj == newValue)` our obj is Culture pl-PL and new value is also pl-PL, we have here == operator, so we are comparing references since CultureInfo did not overload operator... 

...lets check how CultureInfo class is implemented especially GetCultureInfo method from out test Case2...

[quick ILSpy lookup]

...OK, now we know, this method returns the same object for a given culture every time. This is why our test Case1 is working fine, because we are creating CultureInfo every request `var cultureInfo = new CultureInfo(TestCulture);`.

Because of this `if (obj == newValue)` AsyncLocal.OnValueChanged was not called and `Thread.m_CurrentUICulture = value;` was not set and resources are incorrect.


#Question

**where is the error in this case?**

ExecutionContext != null on subsequent request on the same thread, or setting culture like in test Case2?

