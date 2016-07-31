using System;
using AudioToolbox;
using AudioUnit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace SpotifySDK
{
	// typedef void (^SPTErrorableOperationCallback)(NSError *);
	delegate void SPTErrorableOperationCallback (NSError arg0);

	// @protocol SPTTrackProvider <NSObject>
	[Protocol, Model]
	[BaseType (typeof(NSObject))]
	interface SPTTrackProvider
	{
		// @required -(NSArray *)tracksForPlayback;
		[Abstract]
		[Export ("tracksForPlayback")]
		//[Verify (MethodToProperty), Verify (StronglyTypedNSArray)]
		NSObject[] TracksForPlayback { get; }

		// @required -(NSURL *)playableUri;
		[Abstract]
		[Export ("playableUri")]
		//[Verify (MethodToProperty)]
		NSUrl PlayableUri { get; }
	}

	interface ISPTTrackProvider : SPTTrackProvider
	{
	}

	[Static]
	partial interface Constants
	{
		// extern NSString *const SPTAuthStreamingScope;
		[Field ("SPTAuthStreamingScope", "__Internal")]
		NSString SPTAuthStreamingScope { get; }

		// extern NSString *const SPTAuthPlaylistReadPrivateScope;
		[Field ("SPTAuthPlaylistReadPrivateScope", "__Internal")]
		NSString SPTAuthPlaylistReadPrivateScope { get; }

		// extern NSString *const SPTAuthPlaylistModifyPublicScope;
		[Field ("SPTAuthPlaylistModifyPublicScope", "__Internal")]
		NSString SPTAuthPlaylistModifyPublicScope { get; }

		// extern NSString *const SPTAuthPlaylistModifyPrivateScope;
		[Field ("SPTAuthPlaylistModifyPrivateScope", "__Internal")]
		NSString SPTAuthPlaylistModifyPrivateScope { get; }

		// extern NSString *const SPTAuthUserFollowModifyScope;
		[Field ("SPTAuthUserFollowModifyScope", "__Internal")]
		NSString SPTAuthUserFollowModifyScope { get; }

		// extern NSString *const SPTAuthUserFollowReadScope;
		[Field ("SPTAuthUserFollowReadScope", "__Internal")]
		NSString SPTAuthUserFollowReadScope { get; }

		// extern NSString *const SPTAuthUserLibraryReadScope;
		[Field ("SPTAuthUserLibraryReadScope", "__Internal")]
		NSString SPTAuthUserLibraryReadScope { get; }

		// extern NSString *const SPTAuthUserLibraryModifyScope;
		[Field ("SPTAuthUserLibraryModifyScope", "__Internal")]
		NSString SPTAuthUserLibraryModifyScope { get; }

		// extern NSString *const SPTAuthUserReadPrivateScope;
		[Field ("SPTAuthUserReadPrivateScope", "__Internal")]
		NSString SPTAuthUserReadPrivateScope { get; }

		// extern NSString *const SPTAuthUserReadBirthDateScope;
		[Field ("SPTAuthUserReadBirthDateScope", "__Internal")]
		NSString SPTAuthUserReadBirthDateScope { get; }

		// extern NSString *const SPTAuthUserReadEmailScope;
		[Field ("SPTAuthUserReadEmailScope", "__Internal")]
		NSString SPTAuthUserReadEmailScope { get; }

		// extern NSString *const SPTAuthSessionUserDefaultsKey;
		// [Field ("SPTAuthSessionUserDefaultsKey", "__Internal")]
		// NSString SPTAuthSessionUserDefaultsKey { get; }
	}

	// @interface SPTAuth : NSObject
	[BaseType (typeof(NSObject))]
	public interface SPTAuth
	{
		// +(SPTAuth *)defaultInstance;
		[Static]
		[Export ("defaultInstance")]
		SPTAuth DefaultInstance { get; }

		[Static]
		[Export("defaultInstance")]
		SPTAuth GetDefaultInstance();

		// @property (readwrite, strong) NSString * clientID;
		[Export ("clientID", ArgumentSemantic.Strong)]
		string ClientID { get; set; }

		// @property (readwrite, strong) NSURL * redirectURL;
		[Export ("redirectURL", ArgumentSemantic.Strong)]
		NSUrl RedirectURL { get; set; }

		// @property (readwrite, strong) NSArray * requestedScopes;
		[Export ("requestedScopes", ArgumentSemantic.Strong)]
		// [Verify (StronglyTypedNSArray)]
		NSObject[] RequestedScopes { get; set; }

		// @property (readwrite, strong) SPTSession * session;
		[Export ("session", ArgumentSemantic.Strong)]
		SPTSession Session { get; set; }

		// @property (readwrite, strong) NSString * sessionUserDefaultsKey;
		[Export ("sessionUserDefaultsKey", ArgumentSemantic.Strong)]
		string SessionUserDefaultsKey { get; set; }

		// @property (readwrite, strong) NSURL * tokenSwapURL;
		[Export ("tokenSwapURL", ArgumentSemantic.Strong)]
		NSUrl TokenSwapURL { get; set; }

		// @property (readwrite, strong) NSURL * tokenRefreshURL;
		[Export ("tokenRefreshURL", ArgumentSemantic.Strong)]
		NSUrl TokenRefreshURL { get; set; }

		// @property (readonly) BOOL hasTokenSwapService;
		[Export ("hasTokenSwapService")]
		bool HasTokenSwapService { get; }

		// @property (readonly) BOOL hasTokenRefreshService;
		[Export ("hasTokenRefreshService")]
		bool HasTokenRefreshService { get; }

		// @property (readonly) NSURL * loginURL;
		[Export ("loginURL")]
		NSUrl LoginURL { get; }

		// +(NSURL *)loginURLForClientId:(NSString *)clientId withRedirectURL:(NSURL *)redirectURL scopes:(NSArray *)scopes responseType:(NSString *)responseType;
		[Static]
		[Export ("loginURLForClientId:withRedirectURL:scopes:responseType:")]
		// [Verify (StronglyTypedNSArray)]
		NSUrl LoginURLForClientId (string clientId, NSUrl redirectURL, NSObject[] scopes, string responseType);

		// -(BOOL)canHandleURL:(NSURL *)callbackURL;
		[Export ("canHandleURL:")]
		bool CanHandleURL (NSUrl callbackURL);

		// -(void)handleAuthCallbackWithTriggeredAuthURL:(NSURL *)url callback:(SPTAuthCallback)block;
		[Export ("handleAuthCallbackWithTriggeredAuthURL:callback:")]
		void HandleAuthCallbackWithTriggeredAuthURL (NSUrl url, SPTAuthCallback block);

		// +(BOOL)supportsApplicationAuthentication;
		[Static]
		[Export ("supportsApplicationAuthentication")]
		// [Verify (MethodToProperty)]
		bool SupportsApplicationAuthentication { get; }

		// +(BOOL)spotifyApplicationIsInstalled;
		[Static]
		[Export ("spotifyApplicationIsInstalled")]
		// [Verify (MethodToProperty)]
		bool SpotifyApplicationIsInstalled { get; }

		// -(void)renewSession:(SPTSession *)session callback:(SPTAuthCallback)block;
		[Export ("renewSession:callback:")]
		void RenewSession (SPTSession session, SPTAuthCallback block);
	}

	// typedef void (^SPTAuthCallback)(NSError *, SPTSession *)
	public delegate void SPTAuthCallback (NSError arg0, SPTSession arg1);

	// @interface SPTSession : NSObject <NSSecureCoding>
	[BaseType (typeof(NSObject))]
	public interface SPTSession : INSSecureCoding
	{
		// -(instancetype)initWithUserName:(NSString *)userName accessToken:(NSString *)accessToken expirationDate:(NSDate *)expirationDate;
		[Export ("initWithUserName:accessToken:expirationDate:")]
		IntPtr Constructor (string userName, string accessToken, NSDate expirationDate);

		// -(instancetype)initWithUserName:(NSString *)userName accessToken:(NSString *)accessToken encryptedRefreshToken:(NSString *)encryptedRefreshToken expirationDate:(NSDate *)expirationDate;
		[Export ("initWithUserName:accessToken:encryptedRefreshToken:expirationDate:")]
		IntPtr Constructor (string userName, string accessToken, string encryptedRefreshToken, NSDate expirationDate);

		// -(instancetype)initWithUserName:(NSString *)userName accessToken:(NSString *)accessToken expirationTimeInterval:(NSTimeInterval)timeInterval;
		[Export ("initWithUserName:accessToken:expirationTimeInterval:")]
		IntPtr Constructor (string userName, string accessToken, double timeInterval);

		// -(BOOL)isValid;
		[Export ("isValid")]
		// [Verify (MethodToProperty)]
		bool IsValid { get; }

		// @property (readonly, copy, nonatomic) NSString * canonicalUsername;
		[Export ("canonicalUsername")]
		string CanonicalUsername { get; }

		// @property (readonly, copy, nonatomic) NSString * accessToken;
		[Export ("accessToken")]
		string AccessToken { get; }

		// @property (readonly, copy, nonatomic) NSString * encryptedRefreshToken;
		[Export ("encryptedRefreshToken")]
		string EncryptedRefreshToken { get; }

		// @property (readonly, copy, nonatomic) NSDate * expirationDate;
		[Export ("expirationDate", ArgumentSemantic.Copy)]
		NSDate ExpirationDate { get; }

		// @property (readonly, copy, nonatomic) NSString * tokenType;
		[Export ("tokenType")]
		string TokenType { get; }
	}

	// @interface SPTConnectButton : UIControl
	[BaseType (typeof(UIControl))]
	interface SPTConnectButton
	{
	}

	// @protocol SPTAuthViewDelegate
	[Protocol, 
	 BaseType(typeof(NSObject)), 
	 Model]
	public interface SPTAuthViewDelegate
	{
		// @required -(void)authenticationViewController:(SPTAuthViewController *)authenticationViewController didLoginWithSession:(SPTSession *)session;
		[Abstract]
		[Export ("authenticationViewController:didLoginWithSession:")]
		void AuthenticationViewControllerLogin (SPTAuthViewController authenticationViewController, SPTSession session);

		// @required -(void)authenticationViewController:(SPTAuthViewController *)authenticationViewController didFailToLogin:(NSError *)error;
		[Abstract]
		[Export ("authenticationViewController:didFailToLogin:")]
		void AuthenticationViewControllerFail (SPTAuthViewController authenticationViewController, NSError error);

		// @required -(void)authenticationViewControllerDidCancelLogin:(SPTAuthViewController *)authenticationViewController;
		[Abstract]
		[Export ("authenticationViewControllerDidCancelLogin:")]
		void AuthenticationViewControllerDidCancelLogin (SPTAuthViewController authenticationViewController);
	}

	// @interface SPTAuthViewController : UIViewController
	[BaseType (typeof(UIViewController))]
	public interface SPTAuthViewController
	{
		[Wrap ("WeakDelegate")]
		SPTAuthViewDelegate Delegate { get; set; }

		// @property (assign, nonatomic) id<SPTAuthViewDelegate> delegate;
		[NullAllowed, Export ("delegate", ArgumentSemantic.Assign)]
		NSObject WeakDelegate { get; set; }

		// @property (readwrite, nonatomic) BOOL hideSignup;
		[Export ("hideSignup")]
		bool HideSignup { get; set; }

		// +(SPTAuthViewController *)authenticationViewController;
		[Static]
		[Export ("authenticationViewController")]
		// [Verify (MethodToProperty)]
		SPTAuthViewController AuthenticationViewController { get; }

		// +(SPTAuthViewController *)authenticationViewControllerWithAuth:(SPTAuth *)auth;
		[Static]
		[Export ("authenticationViewControllerWithAuth:")]
		SPTAuthViewController AuthenticationViewControllerWithAuth (SPTAuth auth);

		// -(void)clearCookies:(void (^)())callback;
		[Export ("clearCookies:")]
		void ClearCookies (Action callback);
	}

	// @protocol SPTJSONObject <NSObject>
	[Protocol, Model]
	[BaseType (typeof(NSObject))]
	interface SPTJSONObject
	{
		// @required -(id)initWithDecodedJSONObject:(id)decodedObject error:(NSError **)error;
		[Abstract]
		[Export ("initWithDecodedJSONObject:error:")]
		IntPtr Error (NSObject decodedObject, out NSError error);

		// @required @property (readonly, copy, nonatomic) id decodedJSONObject;
		[Abstract]
		[Export ("decodedJSONObject", ArgumentSemantic.Copy)]
		NSObject DecodedJSONObject { get; }
	}

	interface ISPTJSONObject : SPTJSONObject
	{
	}

	// @interface SPTJSONDecoding : NSObject
	[BaseType (typeof(NSObject))]
	interface SPTJSONDecoding
	{
		// +(id)SPObjectFromDecodedJSON:(id)decodedJson error:(NSError **)error;
		[Static]
		[Export ("SPObjectFromDecodedJSON:error:")]
		NSObject SPObjectFromDecodedJSON (NSObject decodedJson, out NSError error);

		// +(id)SPObjectFromEncodedJSON:(NSData *)json error:(NSError **)error;
		[Static]
		[Export ("SPObjectFromEncodedJSON:error:")]
		NSObject SPObjectFromEncodedJSON (NSData json, out NSError error);

		// +(id)partialSPObjectFromDecodedJSON:(id)decodedJson error:(NSError **)error;
		[Static]
		[Export ("partialSPObjectFromDecodedJSON:error:")]
		NSObject PartialSPObjectFromDecodedJSON (NSObject decodedJson, out NSError error);

		// +(id)partialSPObjectFromEncodedJSON:(NSData *)json error:(NSError **)error;
		[Static]
		[Export ("partialSPObjectFromEncodedJSON:error:")]
		NSObject PartialSPObjectFromEncodedJSON (NSData json, out NSError error);
	}

	// @interface SPTJSONObjectBase : NSObject <SPTJSONObject>
	[BaseType (typeof(NSObject))]
	interface SPTJSONObjectBase : ISPTJSONObject
	{
		// @property (readwrite, copy, nonatomic) id decodedJSONObject;
		[Export ("decodedJSONObject", ArgumentSemantic.Copy)]
		NSObject DecodedJSONObject { get; set; }
	}

	// @protocol SPTPartialObject <NSObject>
	[Protocol, Model]
	[BaseType (typeof(NSObject))]
	interface SPTPartialObject
	{
		// @required @property (readonly, copy, nonatomic) NSString * name;
		[Abstract]
		[Export ("name")]
		string Name { get; }

		// @required @property (readonly, copy, nonatomic) NSURL * uri;
		[Abstract]
		[Export ("uri", ArgumentSemantic.Copy)]
		NSUrl Uri { get; }
	}

	interface ISPTPartialObject : SPTPartialObject
	{
	}

	// typedef void (^SPTRequestCallback)(NSError *, id);
	delegate void SPTRequestCallback (NSError arg0, NSObject arg1);

	// typedef void (^SPTRequestDataCallback)(NSError *, NSURLResponse *, NSData *);
	delegate void SPTRequestDataCallback (NSError arg0, NSUrlResponse arg1, NSData arg2);

	partial interface Constants
	{
		// extern NSString *const SPTMarketFromToken;
		[Field ("SPTMarketFromToken", "__Internal")]
		NSString SPTMarketFromToken { get; }
	}

	// @protocol SPTRequestHandlerProtocol
	[Protocol, BaseType(typeof(NSObject)), Model]
	interface SPTRequestHandlerProtocol
	{
		// @required -(void)performRequest:(NSURLRequest *)request callback:(SPTRequestDataCallback)block;
		// [Abstract]
		[Export ("performRequest:callback:")]
		void Callback (NSUrlRequest request, SPTRequestDataCallback block);
	}

	// @interface SPTRequest : NSObject
	[BaseType (typeof(NSObject))]
	interface SPTRequest
	{
		// +(id<SPTRequestHandlerProtocol>)sharedHandler;
		// +(void)setSharedHandler:(id<SPTRequestHandlerProtocol>)handler;
		[Static]
		[Export ("sharedHandler")]
		// [Verify (MethodToProperty)]
		SPTRequestHandlerProtocol SPTRequestHandlerProtocol { get; set; }

		// +(void)requestItemAtURI:(NSURL *)uri withSession:(SPTSession *)session callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("requestItemAtURI:withSession:callback:")]
		void RequestItemAtURI (NSUrl uri, SPTSession session, SPTRequestCallback block);

		// +(void)requestItemAtURI:(NSURL *)uri withSession:(SPTSession *)session market:(NSString *)market callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("requestItemAtURI:withSession:market:callback:")]
		void RequestItemAtURI (NSUrl uri, SPTSession session, string market, SPTRequestCallback block);

		// +(void)requestItemFromPartialObject:(id<SPTPartialObject>)partialObject withSession:(SPTSession *)session callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("requestItemFromPartialObject:withSession:callback:")]
		void RequestItemFromPartialObject (SPTPartialObject partialObject, SPTSession session, SPTRequestCallback block);

		// +(void)requestItemFromPartialObject:(id<SPTPartialObject>)partialObject withSession:(SPTSession *)session market:(NSString *)market callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("requestItemFromPartialObject:withSession:market:callback:")]
		void RequestItemFromPartialObject (SPTPartialObject partialObject, SPTSession session, string market, SPTRequestCallback block);

		// +(NSURLRequest *)createRequestForURL:(NSURL *)url withAccessToken:(NSString *)accessToken httpMethod:(NSString *)httpMethod values:(id)values valueBodyIsJSON:(BOOL)encodeAsJSON sendDataAsQueryString:(BOOL)dataAsQueryString error:(NSError **)error;
		[Static]
		[Export ("createRequestForURL:withAccessToken:httpMethod:values:valueBodyIsJSON:sendDataAsQueryString:error:")]
		NSUrlRequest CreateRequestForURL (NSUrl url, string accessToken, string httpMethod, NSObject values, bool encodeAsJSON, bool dataAsQueryString, out NSError error);

		// +(void)playlistsForUserInSession:(SPTSession *)session callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("playlistsForUserInSession:callback:")]
		void PlaylistsForUserInSession (SPTSession session, SPTRequestCallback block);

		// +(void)playlistsForUser:(NSString *)username withSession:(SPTSession *)session callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("playlistsForUser:withSession:callback:")]
		void PlaylistsForUser (string username, SPTSession session, SPTRequestCallback block);

		// +(void)starredListForUserInSession:(SPTSession *)session callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("starredListForUserInSession:callback:")]
		void StarredListForUserInSession (SPTSession session, SPTRequestCallback block);

		// +(void)userInformationForUserInSession:(SPTSession *)session callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("userInformationForUserInSession:callback:")]
		void UserInformationForUserInSession (SPTSession session, SPTRequestCallback block);

		// +(void)performSearchWithQuery:(NSString *)searchQuery queryType:(SPTSearchQueryType)searchQueryType offset:(NSInteger)offset session:(SPTSession *)session market:(NSString *)market callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("performSearchWithQuery:queryType:offset:session:market:callback:")]
		void PerformSearchWithQuery (string searchQuery, SPTSearchQueryType searchQueryType, nint offset, SPTSession session, string market, SPTRequestCallback block);

		// +(void)performSearchWithQuery:(NSString *)searchQuery queryType:(SPTSearchQueryType)searchQueryType session:(SPTSession *)session market:(NSString *)market callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("performSearchWithQuery:queryType:session:market:callback:")]
		void PerformSearchWithQuery (string searchQuery, SPTSearchQueryType searchQueryType, SPTSession session, string market, SPTRequestCallback block);

		// +(void)performSearchWithQuery:(NSString *)searchQuery queryType:(SPTSearchQueryType)searchQueryType offset:(NSInteger)offset session:(SPTSession *)session callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("performSearchWithQuery:queryType:offset:session:callback:")]
		void PerformSearchWithQuery (string searchQuery, SPTSearchQueryType searchQueryType, nint offset, SPTSession session, SPTRequestCallback block);

		// +(void)performSearchWithQuery:(NSString *)searchQuery queryType:(SPTSearchQueryType)searchQueryType session:(SPTSession *)session callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("performSearchWithQuery:queryType:session:callback:")]
		void PerformSearchWithQuery (string searchQuery, SPTSearchQueryType searchQueryType, SPTSession session, SPTRequestCallback block);

		// +(void)savedTracksForUserInSession:(SPTSession *)session callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("savedTracksForUserInSession:callback:")]
		void SavedTracksForUserInSession (SPTSession session, SPTRequestCallback block);

		// +(void)saveTracks:(NSArray *)tracks forUserInSession:(SPTSession *)session callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("saveTracks:forUserInSession:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void SaveTracks (NSObject[] tracks, SPTSession session, SPTRequestCallback block);

		// +(void)savedTracksContains:(NSArray *)tracks forUserInSession:(SPTSession *)session callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("savedTracksContains:forUserInSession:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void SavedTracksContains (NSObject[] tracks, SPTSession session, SPTRequestCallback block);

		// +(void)removeTracksFromSaved:(NSArray *)tracks forUserInSession:(SPTSession *)session callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("removeTracksFromSaved:forUserInSession:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void RemoveTracksFromSaved (NSObject[] tracks, SPTSession session, SPTRequestCallback block);

		// +(void)requestFeaturedPlaylistsForCountry:(NSString *)country limit:(NSInteger)limit offset:(NSInteger)offset locale:(NSString *)locale timestamp:(NSDate *)timestamp session:(SPTSession *)session callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("requestFeaturedPlaylistsForCountry:limit:offset:locale:timestamp:session:callback:")]
		void RequestFeaturedPlaylistsForCountry (string country, nint limit, nint offset, string locale, NSDate timestamp, SPTSession session, SPTRequestCallback block);

		// +(void)requestNewReleasesForCountry:(NSString *)country limit:(NSInteger)limit offset:(NSInteger)offset session:(SPTSession *)session callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("requestNewReleasesForCountry:limit:offset:session:callback:")]
		void RequestNewReleasesForCountry (string country, nint limit, nint offset, SPTSession session, SPTRequestCallback block);
	}

	// @interface SPTPartialAlbum : SPTJSONObjectBase <SPTPartialObject>
	[BaseType (typeof(SPTJSONObjectBase))]
	interface SPTPartialAlbum : ISPTPartialObject
	{
		// @property (readonly, copy, nonatomic) NSString * identifier;
		[Export ("identifier")]
		string Identifier { get; }

		// @property (readonly, copy, nonatomic) NSString * name;
		[Export ("name")]
		string Name { get; }

		// @property (readonly, copy, nonatomic) NSURL * uri;
		[Export ("uri", ArgumentSemantic.Copy)]
		NSUrl Uri { get; }

		// @property (readonly, copy, nonatomic) NSURL * playableUri;
		[Export ("playableUri", ArgumentSemantic.Copy)]
		NSUrl PlayableUri { get; }

		// @property (readonly, copy, nonatomic) NSURL * sharingURL;
		[Export ("sharingURL", ArgumentSemantic.Copy)]
		NSUrl SharingURL { get; }

		// @property (readonly, copy, nonatomic) NSArray * covers;
		[Export ("covers", ArgumentSemantic.Copy)]
		// [Verify (StronglyTypedNSArray)]
		SPTImage[] Covers { get; }

		// @property (readonly, nonatomic) SPTImage * smallestCover;
		[Export ("smallestCover")]
		SPTImage SmallestCover { get; }

		// @property (readonly, nonatomic) SPTImage * largestCover;
		[Export ("largestCover")]
		SPTImage LargestCover { get; }

		// @property (readonly, nonatomic) SPTAlbumType type;
		[Export ("type")]
		SPTAlbumType Type { get; }

		// @property (readonly, copy, nonatomic) NSArray * availableTerritories;
		[Export ("availableTerritories", ArgumentSemantic.Copy)]
		// [Verify (StronglyTypedNSArray)]
		NSObject[] AvailableTerritories { get; }

		// +(instancetype)partialAlbumFromDecodedJSON:(id)decodedObject error:(NSError **)error;
		[Static]
		[Export ("partialAlbumFromDecodedJSON:error:")]
		SPTPartialAlbum PartialAlbumFromDecodedJSON (NSObject decodedObject, out NSError error);
	}

	// @interface SPTAlbum : SPTPartialAlbum <SPTJSONObject, SPTTrackProvider>
	[BaseType (typeof(SPTPartialAlbum))]
	interface SPTAlbum : ISPTJSONObject, ISPTTrackProvider
	{
		// @property (readonly, copy, nonatomic) NSDictionary * externalIds;
		[Export ("externalIds", ArgumentSemantic.Copy)]
		NSDictionary ExternalIds { get; }

		// @property (readonly, nonatomic) NSArray * artists;
		[Export ("artists")]
		//[Verify (StronglyTypedNSArray)]
		SPTPartialArtist[] Artists { get; }

		// @property (readonly, nonatomic) SPTListPage * firstTrackPage;
		[Export ("firstTrackPage")]
		SPTListPage FirstTrackPage { get; }

		// @property (readonly, nonatomic) NSInteger releaseYear;
		[Export ("releaseYear")]
		nint ReleaseYear { get; }

		// @property (readonly, nonatomic) NSDate * releaseDate;
		[Export ("releaseDate")]
		NSDate ReleaseDate { get; }

		// @property (readonly, copy, nonatomic) NSArray * genres;
		[Export ("genres", ArgumentSemantic.Copy)]
		//[Verify (StronglyTypedNSArray)]
		NSObject[] Genres { get; }

		// @property (readonly, nonatomic) double popularity;
		[Export ("popularity")]
		double Popularity { get; }

		// +(NSURLRequest *)createRequestForAlbum:(NSURL *)uri withAccessToken:(NSString *)accessToken market:(NSString *)market error:(NSError **)error;
		[Static]
		[Export ("createRequestForAlbum:withAccessToken:market:error:")]
		NSUrlRequest CreateRequestForAlbum (NSUrl uri, string accessToken, string market, out NSError error);

		// +(NSURLRequest *)createRequestForAlbums:(NSArray *)uris withAccessToken:(NSString *)accessToken market:(NSString *)market error:(NSError **)error;
		[Static]
		[Export ("createRequestForAlbums:withAccessToken:market:error:")]
		// [Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForAlbums (NSUrl[] uris, string accessToken, string market, out NSError error);

		// +(instancetype)albumFromData:(NSData *)data withResponse:(NSURLResponse *)response error:(NSError **)error;
		[Static]
		[Export ("albumFromData:withResponse:error:")]
		SPTAlbum AlbumFromData (NSData data, NSUrlResponse response, out NSError error);

		// +(instancetype)albumFromDecodedJSON:(id)decodedObject error:(NSError **)error;
		[Static]
		[Export ("albumFromDecodedJSON:error:")]
		SPTAlbum AlbumFromDecodedJSON (NSObject decodedObject, out NSError error);

		// +(NSArray *)albumsFromDecodedJSON:(id)decodedObject error:(NSError **)error;
		[Static]
		[Export ("albumsFromDecodedJSON:error:")]
		// [Verify (StronglyTypedNSArray)]
		SPTAlbum[] AlbumsFromDecodedJSON (NSObject decodedObject, out NSError error);

		// +(void)albumWithURI:(NSURL *)uri session:(SPTSession *)session callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("albumWithURI:session:callback:")]
		void AlbumWithURI (NSUrl uri, SPTSession session, SPTRequestCallback block);

		// +(void)albumWithURI:(NSURL *)uri accessToken:(NSString *)accessToken market:(NSString *)market callback:(SPTRequestCallback)block;
		[Static]
		[Export ("albumWithURI:accessToken:market:callback:")]
		void AlbumWithURI (NSUrl uri, string accessToken, string market, SPTRequestCallback block);

		// +(void)albumsWithURIs:(NSArray *)uris session:(SPTSession *)session callback:(SPTRequestCallback)block __attribute__((deprecated("")));
		[Static]
		[Export ("albumsWithURIs:session:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void AlbumsWithURIs (NSUrl[] uris, SPTSession session, SPTRequestCallback block);

		// +(void)albumsWithURIs:(NSArray *)uris accessToken:(NSString *)accessToken market:(NSString *)market callback:(SPTRequestCallback)block;
		[Static]
		[Export ("albumsWithURIs:accessToken:market:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void AlbumsWithURIs (NSUrl[] uris, string accessToken, string market, SPTRequestCallback block);

		// +(BOOL)isAlbumURI:(NSURL *)uri;
		[Static]
		[Export ("isAlbumURI:")]
		bool IsAlbumURI (NSUrl uri);
	}

	// @interface SPTPartialArtist : SPTJSONObjectBase <SPTPartialObject>
	[BaseType (typeof(SPTJSONObjectBase))]
	interface SPTPartialArtist : ISPTPartialObject
	{
		// @property (readonly, copy, nonatomic) NSString * identifier;
		[Export ("identifier")]
		string Identifier { get; }

		// @property (readonly, copy, nonatomic) NSURL * playableUri;
		[Export ("playableUri", ArgumentSemantic.Copy)]
		NSUrl PlayableUri { get; }

		// @property (readonly, copy, nonatomic) NSURL * sharingURL;
		[Export ("sharingURL", ArgumentSemantic.Copy)]
		NSUrl SharingURL { get; }

		// +(instancetype)partialArtistFromDecodedJSON:(id)decodedObject error:(NSError **)error;
		[Static]
		[Export ("partialArtistFromDecodedJSON:error:")]
		SPTPartialArtist PartialArtistFromDecodedJSON (NSObject decodedObject, out NSError error);
	}

	// @interface SPTArtist : SPTPartialArtist <SPTJSONObject>
	[BaseType (typeof(SPTPartialArtist))]
	interface SPTArtist : ISPTJSONObject
	{
		// @property (readonly, copy, nonatomic) NSDictionary * externalIds;
		[Export ("externalIds", ArgumentSemantic.Copy)]
		NSDictionary ExternalIds { get; }

		// @property (readonly, copy, nonatomic) NSArray * genres;
		[Export ("genres", ArgumentSemantic.Copy)]
		// [Verify (StronglyTypedNSArray)]
		NSObject[] Genres { get; }

		// @property (readonly, copy, nonatomic) NSArray * images;
		[Export ("images", ArgumentSemantic.Copy)]
		// [Verify (StronglyTypedNSArray)]
		SPTImage[] Images { get; }

		// @property (readonly, nonatomic) SPTImage * smallestImage;
		[Export ("smallestImage")]
		SPTImage SmallestImage { get; }

		// @property (readonly, nonatomic) SPTImage * largestImage;
		[Export ("largestImage")]
		SPTImage LargestImage { get; }

		// @property (readonly, nonatomic) double popularity;
		[Export ("popularity")]
		double Popularity { get; }

		// @property (readonly, nonatomic) long followerCount;
		[Export ("followerCount")]
		nint FollowerCount { get; }

		// +(NSURLRequest *)createRequestForArtist:(NSURL *)uri withAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForArtist:withAccessToken:error:")]
		NSUrlRequest CreateRequestForArtist (NSUrl uri, string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForArtists:(NSArray *)uris withAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForArtists:withAccessToken:error:")]
		// [Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForArtists (NSUrl[] uris, string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForAlbumsByArtist:(NSURL *)artist ofType:(SPTAlbumType)type withAccessToken:(NSString *)accessToken market:(NSString *)market error:(NSError **)error;
		[Static]
		[Export ("createRequestForAlbumsByArtist:ofType:withAccessToken:market:error:")]
		NSUrlRequest CreateRequestForAlbumsByArtist (NSUrl artist, SPTAlbumType type, string accessToken, string market, out NSError error);

		// +(NSURLRequest *)createRequestForTopTracksForArtist:(NSURL *)artist withAccessToken:(NSString *)accessToken market:(NSString *)market error:(NSError **)error;
		[Static]
		[Export ("createRequestForTopTracksForArtist:withAccessToken:market:error:")]
		NSUrlRequest CreateRequestForTopTracksForArtist (NSUrl artist, string accessToken, string market, out NSError error);

		// +(NSURLRequest *)createRequestForArtistsRelatedTo:(NSURL *)artist withAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForArtistsRelatedTo:withAccessToken:error:")]
		NSUrlRequest CreateRequestForArtistsRelatedTo (NSUrl artist, string accessToken, out NSError error);

		// +(instancetype)artistFromData:(NSData *)data withResponse:(NSURLResponse *)response error:(NSError **)error;
		[Static]
		[Export ("artistFromData:withResponse:error:")]
		SPTArtist ArtistFromData (NSData data, NSUrlResponse response, out NSError error);

		// +(instancetype)artistFromDecodedJSON:(id)decodedObject error:(NSError **)error;
		[Static]
		[Export ("artistFromDecodedJSON:error:")]
		SPTArtist ArtistFromDecodedJSON (NSObject decodedObject, out NSError error);

		// +(NSArray *)artistsFromData:(NSData *)data withResponse:(NSURLResponse *)response error:(NSError **)error;
		[Static]
		[Export ("artistsFromData:withResponse:error:")]
		// [Verify (StronglyTypedNSArray)]
		SPTArtist[] ArtistsFromData (NSData data, NSUrlResponse response, out NSError error);

		// +(NSArray *)artistsFromDecodedJSON:(id)decodedObject error:(NSError **)error;
		[Static]
		[Export ("artistsFromDecodedJSON:error:")]
		// [Verify (StronglyTypedNSArray)]
		SPTArtist[] ArtistsFromDecodedJSON (NSObject decodedObject, out NSError error);

		// +(void)artistWithURI:(NSURL *)uri session:(SPTSession *)session callback:(SPTRequestCallback)block;
		[Static]
		[Export ("artistWithURI:session:callback:")]
		void ArtistWithURI (NSUrl uri, SPTSession session, SPTRequestCallback block);

		// +(void)artistWithURI:(NSURL *)uri accessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Static]
		[Export ("artistWithURI:accessToken:callback:")]
		void ArtistWithURI (NSUrl uri, string accessToken, SPTRequestCallback block);

		// +(void)artistsWithURIs:(NSArray *)uris session:(SPTSession *)session callback:(SPTRequestCallback)block;
		[Static]
		[Export ("artistsWithURIs:session:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void ArtistsWithURIs (NSUrl[] uris, SPTSession session, SPTRequestCallback block);

		// +(void)artistsWithURIs:(NSArray *)uris accessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Static]
		[Export ("artistsWithURIs:accessToken:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void ArtistsWithURIs (NSUrl[] uris, string accessToken, SPTRequestCallback block);

		// -(void)requestAlbumsOfType:(SPTAlbumType)type withSession:(SPTSession *)session availableInTerritory:(NSString *)territory callback:(SPTRequestCallback)block;
		[Export ("requestAlbumsOfType:withSession:availableInTerritory:callback:")]
		void RequestAlbumsOfType (SPTAlbumType type, SPTSession session, string territory, SPTRequestCallback block);

		// -(void)requestAlbumsOfType:(SPTAlbumType)type withAccessToken:(NSString *)accessToken availableInTerritory:(NSString *)territory callback:(SPTRequestCallback)block;
		[Export ("requestAlbumsOfType:withAccessToken:availableInTerritory:callback:")]
		void RequestAlbumsOfType (SPTAlbumType type, string accessToken, string territory, SPTRequestCallback block);

		// -(void)requestTopTracksForTerritory:(NSString *)territory withSession:(SPTSession *)session callback:(SPTRequestCallback)block;
		[Export ("requestTopTracksForTerritory:withSession:callback:")]
		void RequestTopTracksForTerritory (string territory, SPTSession session, SPTRequestCallback block);

		// -(void)requestTopTracksForTerritory:(NSString *)territory withAccessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Export ("requestTopTracksForTerritory:withAccessToken:callback:")]
		void RequestTopTracksForTerritory (string territory, string accessToken, SPTRequestCallback block);

		// -(void)requestRelatedArtistsWithSession:(SPTSession *)session callback:(SPTRequestCallback)block;
		[Export ("requestRelatedArtistsWithSession:callback:")]
		void RequestRelatedArtistsWithSession (SPTSession session, SPTRequestCallback block);

		// -(void)requestRelatedArtistsWithAccessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Export ("requestRelatedArtistsWithAccessToken:callback:")]
		void RequestRelatedArtistsWithAccessToken (string accessToken, SPTRequestCallback block);

		// +(BOOL)isArtistURI:(NSURL *)uri;
		[Static]
		[Export ("isArtistURI:")]
		bool IsArtistURI (NSUrl uri);

		// +(NSString *)identifierFromURI:(NSURL *)uri;
		[Static]
		[Export ("identifierFromURI:")]
		string IdentifierFromURI (NSUrl uri);
	}

	// @interface SPTImage : NSObject
	[BaseType (typeof(NSObject))]
	interface SPTImage
	{
		// @property (readonly, nonatomic) CGSize size;
		[Export ("size")]
		CGSize Size { get; }

		// @property (readonly, copy, nonatomic) NSURL * imageURL;
		[Export ("imageURL", ArgumentSemantic.Copy)]
		NSUrl ImageURL { get; }

		// +(instancetype)imageFromDecodedJSON:(id)decodedObject error:(NSError **)error;
		[Static]
		[Export ("imageFromDecodedJSON:error:")]
		SPTImage ImageFromDecodedJSON (NSObject decodedObject, out NSError error);
	}

	// @interface SPTPartialPlaylist : SPTJSONObjectBase <SPTPartialObject, SPTTrackProvider>
	[BaseType (typeof(SPTJSONObjectBase))]
	interface SPTPartialPlaylist : ISPTPartialObject, ISPTTrackProvider
	{
		// @property (readonly, copy, nonatomic) NSString * name;
		[Export ("name")]
		string Name { get; }

		// @property (readonly, copy, nonatomic) NSURL * uri;
		[Export ("uri", ArgumentSemantic.Copy)]
		NSUrl Uri { get; }

		// @property (readonly, copy, nonatomic) NSURL * playableUri;
		[Export ("playableUri", ArgumentSemantic.Copy)]
		NSUrl PlayableUri { get; }

		// @property (readonly, nonatomic) SPTUser * owner;
		[Export ("owner")]
		SPTUser Owner { get; }

		// @property (readonly, nonatomic) BOOL isCollaborative;
		[Export ("isCollaborative")]
		bool IsCollaborative { get; }

		// @property (readonly, nonatomic) BOOL isPublic;
		[Export ("isPublic")]
		bool IsPublic { get; }

		// @property (readonly, nonatomic) NSUInteger trackCount;
		[Export ("trackCount")]
		nuint TrackCount { get; }

		// @property (readonly, copy, nonatomic) NSArray * images;
		[Export ("images", ArgumentSemantic.Copy)]
		// [Verify (StronglyTypedNSArray)]
		SPTImage[] Images { get; }

		// @property (readonly, nonatomic) SPTImage * smallestImage;
		[Export ("smallestImage")]
		SPTImage SmallestImage { get; }

		// @property (readonly, nonatomic) SPTImage * largestImage;
		[Export ("largestImage")]
		SPTImage LargestImage { get; }

		// +(instancetype)partialPlaylistFromDecodedJSON:(id)decodedObject error:(NSError **)error;
		[Static]
		[Export ("partialPlaylistFromDecodedJSON:error:")]
		SPTPartialPlaylist PartialPlaylistFromDecodedJSON (NSObject decodedObject, out NSError error);
	}

	// @interface SPTPartialTrack : SPTJSONObjectBase <SPTPartialObject, SPTTrackProvider>
	[BaseType (typeof(SPTJSONObjectBase))]
	interface SPTPartialTrack : ISPTPartialObject, ISPTTrackProvider
	{
		// @property (readonly, copy, nonatomic) NSString * identifier;
		[Export ("identifier")]
		string Identifier { get; }

		// @property (readonly, copy, nonatomic) NSString * name;
		[Export ("name")]
		string Name { get; }

		// @property (readonly, copy, nonatomic) NSURL * playableUri;
		[Export ("playableUri", ArgumentSemantic.Copy)]
		NSUrl PlayableUri { get; }

		// @property (readonly, copy, nonatomic) NSURL * sharingURL;
		[Export ("sharingURL", ArgumentSemantic.Copy)]
		NSUrl SharingURL { get; }

		// @property (readonly, nonatomic) NSTimeInterval duration;
		[Export ("duration")]
		double Duration { get; }

		// @property (readonly, copy, nonatomic) NSArray * artists;
		[Export ("artists", ArgumentSemantic.Copy)]
		// [Verify (StronglyTypedNSArray)]
		SPTArtist[] Artists { get; }

		// @property (readonly, nonatomic) NSInteger discNumber;
		[Export ("discNumber")]
		nint DiscNumber { get; }

		// @property (readonly, nonatomic) BOOL flaggedExplicit;
		[Export ("flaggedExplicit")]
		bool FlaggedExplicit { get; }

		// @property (readonly, nonatomic) BOOL isPlayable;
		[Export ("isPlayable")]
		bool IsPlayable { get; }

		// @property (readonly, nonatomic) BOOL hasPlayable;
		[Export ("hasPlayable")]
		bool HasPlayable { get; }

		// @property (readonly, copy, nonatomic) NSArray * availableTerritories;
		[Export ("availableTerritories", ArgumentSemantic.Copy)]
		// [Verify (StronglyTypedNSArray)]
		NSObject[] AvailableTerritories { get; }

		// @property (readonly, copy, nonatomic) NSURL * previewURL;
		[Export ("previewURL", ArgumentSemantic.Copy)]
		NSUrl PreviewURL { get; }

		// @property (readonly, nonatomic) NSInteger trackNumber;
		[Export ("trackNumber")]
		nint TrackNumber { get; }

		// @property (readonly, nonatomic, strong) SPTPartialAlbum * album;
		[Export ("album", ArgumentSemantic.Strong)]
		SPTPartialAlbum Album { get; }

		// +(instancetype)partialTrackFromDecodedJSON:(id)decodedObject error:(NSError **)error;
		[Static]
		[Export ("partialTrackFromDecodedJSON:error:")]
		SPTPartialTrack PartialTrackFromDecodedJSON (NSObject decodedObject, out NSError error);
	}

	partial interface Constants
	{
		// extern NSString *const SPTPlaylistSnapshotPublicKey;
		[Field ("SPTPlaylistSnapshotPublicKey", "__Internal")]
		NSString SPTPlaylistSnapshotPublicKey { get; }

		// extern NSString *const SPTPlaylistSnapshotNameKey;
		[Field ("SPTPlaylistSnapshotNameKey", "__Internal")]
		NSString SPTPlaylistSnapshotNameKey { get; }
	}

	// @interface SPTPlaylistSnapshot : SPTPartialPlaylist <SPTJSONObject>
	[BaseType (typeof(SPTPartialPlaylist))]
	interface SPTPlaylistSnapshot : ISPTJSONObject
	{
		// @property (readonly, nonatomic) SPTListPage * firstTrackPage;
		[Export ("firstTrackPage")]
		SPTListPage FirstTrackPage { get; }

		// @property (readonly, copy, nonatomic) NSString * snapshotId;
		[Export ("snapshotId")]
		string SnapshotId { get; }

		// @property (readonly, nonatomic) long followerCount;
		[Export ("followerCount")]
		nint FollowerCount { get; }

		// @property (readonly, copy, nonatomic) NSString * descriptionText;
		[Export ("descriptionText")]
		string DescriptionText { get; }

		// +(void)playlistWithURI:(NSURL *)uri session:(SPTSession *)session callback:(SPTRequestCallback)block;
		[Static]
		[Export ("playlistWithURI:session:callback:")]
		void PlaylistWithURI (NSUrl uri, SPTSession session, SPTRequestCallback block);

		// +(void)playlistWithURI:(NSURL *)uri accessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Static]
		[Export ("playlistWithURI:accessToken:callback:")]
		void PlaylistWithURI (NSUrl uri, string accessToken, SPTRequestCallback block);

		// +(void)playlistsWithURIs:(NSArray *)uris session:(SPTSession *)session callback:(SPTRequestCallback)block;
		[Static]
		[Export ("playlistsWithURIs:session:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void PlaylistsWithURIs (NSUrl[] uris, SPTSession session, SPTRequestCallback block);

		// +(BOOL)isPlaylistURI:(NSURL *)uri;
		[Static]
		[Export ("isPlaylistURI:")]
		bool IsPlaylistURI (NSUrl uri);

		// +(BOOL)isStarredURI:(NSURL *)uri;
		[Static]
		[Export ("isStarredURI:")]
		bool IsStarredURI (NSUrl uri);

		// +(void)requestStarredListForUserWithSession:(SPTSession *)session callback:(SPTRequestCallback)block;
		[Static]
		[Export ("requestStarredListForUserWithSession:callback:")]
		void RequestStarredListForUserWithSession (SPTSession session, SPTRequestCallback block);

		// +(void)requestStarredListForUser:(NSString *)username withAccessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Static]
		[Export ("requestStarredListForUser:withAccessToken:callback:")]
		void RequestStarredListForUser (string username, string accessToken, SPTRequestCallback block);

		// -(void)addTracksToPlaylist:(NSArray *)tracks withSession:(SPTSession *)session callback:(SPTErrorableOperationCallback)block;
		[Export ("addTracksToPlaylist:withSession:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void AddTracksToPlaylist (NSObject[] tracks, SPTSession session, SPTErrorableOperationCallback block);

		// -(void)addTracksToPlaylist:(NSArray *)tracks withAccessToken:(NSString *)accessToken callback:(SPTErrorableOperationCallback)block;
		[Export ("addTracksToPlaylist:withAccessToken:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void AddTracksToPlaylist (NSObject[] tracks, string accessToken, SPTErrorableOperationCallback block);

		// -(void)addTracksWithPositionToPlaylist:(NSArray *)tracks withPosition:(int)position session:(SPTSession *)session callback:(SPTErrorableOperationCallback)block;
		[Export ("addTracksWithPositionToPlaylist:withPosition:session:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void AddTracksWithPositionToPlaylist (NSObject[] tracks, int position, SPTSession session, SPTErrorableOperationCallback block);

		// -(void)addTracksWithPositionToPlaylist:(NSArray *)tracks withPosition:(int)position accessToken:(NSString *)accessToken callback:(SPTErrorableOperationCallback)block;
		[Export ("addTracksWithPositionToPlaylist:withPosition:accessToken:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void AddTracksWithPositionToPlaylist (NSObject[] tracks, int position, string accessToken, SPTErrorableOperationCallback block);

		// -(void)replaceTracksInPlaylist:(NSArray *)tracks withAccessToken:(NSString *)accessToken callback:(SPTErrorableOperationCallback)block;
		[Export ("replaceTracksInPlaylist:withAccessToken:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void ReplaceTracksInPlaylist (NSObject[] tracks, string accessToken, SPTErrorableOperationCallback block);

		// -(void)changePlaylistDetails:(NSDictionary *)data withAccessToken:(NSString *)accessToken callback:(SPTErrorableOperationCallback)block;
		[Export ("changePlaylistDetails:withAccessToken:callback:")]
		void ChangePlaylistDetails (NSDictionary data, string accessToken, SPTErrorableOperationCallback block);

		// -(void)removeTracksFromPlaylist:(NSArray *)tracks withAccessToken:(NSString *)accessToken callback:(SPTErrorableOperationCallback)block;
		[Export ("removeTracksFromPlaylist:withAccessToken:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void RemoveTracksFromPlaylist (NSObject[] tracks, string accessToken, SPTErrorableOperationCallback block);

		// -(void)removeTracksWithPositionsFromPlaylist:(NSArray *)tracks withAccessToken:(NSString *)accessToken callback:(SPTErrorableOperationCallback)block;
		[Export ("removeTracksWithPositionsFromPlaylist:withAccessToken:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void RemoveTracksWithPositionsFromPlaylist (NSObject[] tracks, string accessToken, SPTErrorableOperationCallback block);

		// +(NSURLRequest *)createRequestForAddingTracks:(NSArray *)tracks toPlaylist:(NSURL *)playlist withAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForAddingTracks:toPlaylist:withAccessToken:error:")]
		// Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForAddingTracks (NSObject[] tracks, NSUrl playlist, string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForAddingTracks:(NSArray *)tracks atPosition:(int)position toPlaylist:(NSURL *)playlist withAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForAddingTracks:atPosition:toPlaylist:withAccessToken:error:")]
		// [Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForAddingTracks (NSObject[] tracks, int position, NSUrl playlist, string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForSettingTracks:(NSArray *)tracks inPlaylist:(NSURL *)playlist withAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForSettingTracks:inPlaylist:withAccessToken:error:")]
		//[Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForSettingTracks (NSObject[] tracks, NSUrl playlist, string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForChangingDetails:(NSDictionary *)data inPlaylist:(NSURL *)playlist withAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForChangingDetails:inPlaylist:withAccessToken:error:")]
		NSUrlRequest CreateRequestForChangingDetails (NSDictionary data, NSUrl playlist, string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForRemovingTracksWithPositions:(NSArray *)tracks fromPlaylist:(NSURL *)playlist withAccessToken:(NSString *)accessToken snapshot:(NSString *)snapshotId error:(NSError **)error;
		[Static]
		[Export ("createRequestForRemovingTracksWithPositions:fromPlaylist:withAccessToken:snapshot:error:")]
		// [Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForRemovingTracksWithPositions (NSObject[] tracks, NSUrl playlist, string accessToken, string snapshotId, out NSError error);

		// +(NSURLRequest *)createRequestForRemovingTracks:(NSArray *)tracks fromPlaylist:(NSURL *)playlist withAccessToken:(NSString *)accessToken snapshot:(NSString *)snapshotId error:(NSError **)error;
		[Static]
		[Export ("createRequestForRemovingTracks:fromPlaylist:withAccessToken:snapshot:error:")]
		// [Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForRemovingTracks (NSObject[] tracks, NSUrl playlist, string accessToken, string snapshotId, out NSError error);

		// +(NSURLRequest *)createRequestForPlaylistWithURI:(NSURL *)uri accessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForPlaylistWithURI:accessToken:error:")]
		NSUrlRequest CreateRequestForPlaylistWithURI (NSUrl uri, string accessToken, out NSError error);

		// +(instancetype)playlistSnapshotFromData:(NSData *)data withResponse:(NSURLResponse *)response error:(NSError **)error;
		[Static]
		[Export ("playlistSnapshotFromData:withResponse:error:")]
		SPTPlaylistSnapshot PlaylistSnapshotFromData (NSData data, NSUrlResponse response, out NSError error);

		// +(instancetype)playlistSnapshotFromDecodedJSON:(id)decodedObject error:(NSError **)error;
		[Static]
		[Export ("playlistSnapshotFromDecodedJSON:error:")]
		SPTPlaylistSnapshot PlaylistSnapshotFromDecodedJSON (NSObject decodedObject, out NSError error);
	}

	// @interface SPTListPage : NSObject <SPTTrackProvider>
	[BaseType (typeof(NSObject))]
	interface SPTListPage : ISPTTrackProvider
	{
		// @property (readonly, nonatomic) NSRange range;
		[Export ("range")]
		NSRange Range { get; }

		// @property (readonly, nonatomic) NSUInteger totalListLength;
		[Export ("totalListLength")]
		nuint TotalListLength { get; }

		// @property (readonly, nonatomic) BOOL hasNextPage;
		[Export ("hasNextPage")]
		bool HasNextPage { get; }

		// @property (readonly, nonatomic) BOOL hasPreviousPage;
		[Export ("hasPreviousPage")]
		bool HasPreviousPage { get; }

		// @property (readonly, copy, nonatomic) NSURL * nextPageURL;
		[Export ("nextPageURL", ArgumentSemantic.Copy)]
		NSUrl NextPageURL { get; }

		// @property (readonly, copy, nonatomic) NSURL * previousPageURL;
		[Export ("previousPageURL", ArgumentSemantic.Copy)]
		NSUrl PreviousPageURL { get; }

		// @property (readonly, nonatomic) BOOL isComplete;
		[Export ("isComplete")]
		bool IsComplete { get; }

		// @property (readonly, copy, nonatomic) NSArray * items;
		[Export ("items", ArgumentSemantic.Copy)]
		// [Verify (StronglyTypedNSArray)]
		NSObject[] Items { get; }

		// -(NSURLRequest *)createRequestForNextPageWithAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Export ("createRequestForNextPageWithAccessToken:error:")]
		NSUrlRequest CreateRequestForNextPageWithAccessToken (string accessToken, out NSError error);

		// -(NSURLRequest *)createRequestForPreviousPageWithAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Export ("createRequestForPreviousPageWithAccessToken:error:")]
		NSUrlRequest CreateRequestForPreviousPageWithAccessToken (string accessToken, out NSError error);

		// +(instancetype)listPageFromData:(NSData *)data withResponse:(NSURLResponse *)response expectingPartialChildren:(BOOL)hasPartialChildren rootObjectKey:(NSString *)rootObjectKey error:(NSError **)error;
		[Static]
		[Export ("listPageFromData:withResponse:expectingPartialChildren:rootObjectKey:error:")]
		SPTListPage ListPageFromData (NSData data, NSUrlResponse response, bool hasPartialChildren, string rootObjectKey, out NSError error);

		// +(instancetype)listPageFromDecodedJSON:(id)decodedObject expectingPartialChildren:(BOOL)hasPartialChildren rootObjectKey:(NSString *)rootObjectKey error:(NSError **)error;
		[Static]
		[Export ("listPageFromDecodedJSON:expectingPartialChildren:rootObjectKey:error:")]
		SPTListPage ListPageFromDecodedJSON (NSObject decodedObject, bool hasPartialChildren, string rootObjectKey, out NSError error);

		// -(instancetype)pageByAppendingPage:(SPTListPage *)nextPage;
		[Export ("pageByAppendingPage:")]
		SPTListPage PageByAppendingPage (SPTListPage nextPage);

		// -(void)requestNextPageWithSession:(SPTSession *)session callback:(SPTRequestCallback)block;
		[Export ("requestNextPageWithSession:callback:")]
		void RequestNextPageWithSession (SPTSession session, SPTRequestCallback block);

		// -(void)requestNextPageWithAccessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Export ("requestNextPageWithAccessToken:callback:")]
		void RequestNextPageWithAccessToken (string accessToken, SPTRequestCallback block);

		// -(void)requestPreviousPageWithSession:(SPTSession *)session callback:(SPTRequestCallback)block;
		[Export ("requestPreviousPageWithSession:callback:")]
		void RequestPreviousPageWithSession (SPTSession session, SPTRequestCallback block);

		// -(void)requestPreviousPageWithAccessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Export ("requestPreviousPageWithAccessToken:callback:")]
		void RequestPreviousPageWithAccessToken (string accessToken, SPTRequestCallback block);
	}

	// typedef void (^SPTPlaylistCreationCallback)(NSError *, SPTPlaylistSnapshot *);
	delegate void SPTPlaylistCreationCallback (NSError arg0, SPTPlaylistSnapshot arg1);

	// @interface SPTPlaylistList : SPTListPage
	[BaseType (typeof(SPTListPage))]
	interface SPTPlaylistList
	{
		// +(void)createPlaylistWithName:(NSString *)name publicFlag:(BOOL)isPublic session:(SPTSession *)session callback:(SPTPlaylistCreationCallback)block;
		[Static]
		[Export ("createPlaylistWithName:publicFlag:session:callback:")]
		void CreatePlaylistWithName (string name, bool isPublic, SPTSession session, SPTPlaylistCreationCallback block);

		// +(void)createPlaylistWithName:(NSString *)name forUser:(NSString *)username publicFlag:(BOOL)isPublic accessToken:(NSString *)accessToken callback:(SPTPlaylistCreationCallback)block;
		[Static]
		[Export ("createPlaylistWithName:forUser:publicFlag:accessToken:callback:")]
		void CreatePlaylistWithName (string name, string username, bool isPublic, string accessToken, SPTPlaylistCreationCallback block);

		// +(void)playlistsForUserWithSession:(SPTSession *)session callback:(SPTRequestCallback)block;
		[Static]
		[Export ("playlistsForUserWithSession:callback:")]
		void PlaylistsForUserWithSession (SPTSession session, SPTRequestCallback block);

		// +(void)playlistsForUser:(NSString *)username withAccessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Static]
		[Export ("playlistsForUser:withAccessToken:callback:")]
		void PlaylistsForUser (string username, string accessToken, SPTRequestCallback block);

		// +(void)playlistsForUser:(NSString *)username withSession:(SPTSession *)session callback:(SPTRequestCallback)block;
		[Static]
		[Export ("playlistsForUser:withSession:callback:")]
		void PlaylistsForUser (string username, SPTSession session, SPTRequestCallback block);

		// +(NSURLRequest *)createRequestForCreatingPlaylistWithName:(NSString *)name forUser:(NSString *)username withPublicFlag:(BOOL)isPublic accessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForCreatingPlaylistWithName:forUser:withPublicFlag:accessToken:error:")]
		NSUrlRequest CreateRequestForCreatingPlaylistWithName (string name, string username, bool isPublic, string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForGettingPlaylistsForUser:(NSString *)username withAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForGettingPlaylistsForUser:withAccessToken:error:")]
		NSUrlRequest CreateRequestForGettingPlaylistsForUser (string username, string accessToken, out NSError error);

		// +(instancetype)playlistListFromData:(NSData *)data withResponse:(NSURLResponse *)response error:(NSError **)error;
		[Static]
		[Export ("playlistListFromData:withResponse:error:")]
		SPTPlaylistList PlaylistListFromData (NSData data, NSUrlResponse response, out NSError error);

		// +(instancetype)playlistListFromDecodedJSON:(id)decodedObject error:(NSError **)error;
		[Static]
		[Export ("playlistListFromDecodedJSON:error:")]
		SPTPlaylistList PlaylistListFromDecodedJSON (NSObject decodedObject, out NSError error);
	}

	// @interface SPTTrack : SPTPartialTrack <SPTJSONObject>
	[BaseType (typeof(SPTPartialTrack))]
	interface SPTTrack : ISPTJSONObject
	{
		// @property (readonly, nonatomic) double popularity;
		[Export ("popularity")]
		double Popularity { get; }

		// @property (readonly, copy, nonatomic) NSDictionary * externalIds;
		[Export ("externalIds", ArgumentSemantic.Copy)]
		NSDictionary ExternalIds { get; }

		// +(NSURLRequest *)createRequestForTrack:(NSURL *)uri withAccessToken:(NSString *)accessToken market:(NSString *)market error:(NSError **)error;
		[Static]
		[Export ("createRequestForTrack:withAccessToken:market:error:")]
		NSUrlRequest CreateRequestForTrack (NSUrl uri, string accessToken, string market, out NSError error);

		// +(NSURLRequest *)createRequestForTracks:(NSArray *)uris withAccessToken:(NSString *)accessToken market:(NSString *)market error:(NSError **)error;
		[Static]
		[Export ("createRequestForTracks:withAccessToken:market:error:")]
		// [Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForTracks (NSUrl[] uris, string accessToken, string market, out NSError error);

		// +(instancetype)trackFromData:(NSData *)data withResponse:(NSURLResponse *)response error:(NSError **)error;
		[Static]
		[Export ("trackFromData:withResponse:error:")]
		SPTTrack TrackFromData (NSData data, NSUrlResponse response, out NSError error);

		// +(instancetype)trackFromDecodedJSON:(id)decodedObject error:(NSError **)error;
		[Static]
		[Export ("trackFromDecodedJSON:error:")]
		SPTTrack TrackFromDecodedJSON (NSObject decodedObject, out NSError error);

		// +(NSArray *)tracksFromData:(NSData *)data withResponse:(NSURLResponse *)response error:(NSError **)error;
		[Static]
		[Export ("tracksFromData:withResponse:error:")]
		// [Verify (StronglyTypedNSArray)]
		SPTTrack[] TracksFromData (NSData data, NSUrlResponse response, out NSError error);

		// +(NSArray *)tracksFromDecodedJSON:(id)decodedObject error:(NSError **)error;
		[Static]
		[Export ("tracksFromDecodedJSON:error:")]
		// [Verify (StronglyTypedNSArray)]
		SPTTrack[] TracksFromDecodedJSON (NSObject decodedObject, out NSError error);

		// +(void)trackWithURI:(NSURL *)uri session:(SPTSession *)session callback:(SPTRequestCallback)block;
		[Static]
		[Export ("trackWithURI:session:callback:")]
		void TrackWithURI (NSUrl uri, SPTSession session, SPTRequestCallback block);

		// +(void)trackWithURI:(NSURL *)uri accessToken:(NSString *)accessToken market:(NSString *)market callback:(SPTRequestCallback)block;
		[Static]
		[Export ("trackWithURI:accessToken:market:callback:")]
		void TrackWithURI (NSUrl uri, string accessToken, string market, SPTRequestCallback block);

		// +(void)tracksWithURIs:(NSArray *)uris session:(SPTSession *)session callback:(SPTRequestCallback)block;
		[Static]
		[Export ("tracksWithURIs:session:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void TracksWithURIs (NSUrl[] uris, SPTSession session, SPTRequestCallback block);

		// +(void)tracksWithURIs:(NSArray *)uris accessToken:(NSString *)accessToken market:(NSString *)market callback:(SPTRequestCallback)block;
		[Static]
		[Export ("tracksWithURIs:accessToken:market:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void TracksWithURIs (NSUrl[] uris, string accessToken, string market, SPTRequestCallback block);

		// +(BOOL)isTrackURI:(NSURL *)uri;
		[Static]
		[Export ("isTrackURI:")]
		bool IsTrackURI (NSUrl uri);

		// +(NSString *)identifierFromURI:(NSURL *)uri;
		[Static]
		[Export ("identifierFromURI:")]
		string IdentifierFromURI (NSUrl uri);

		// +(NSArray *)identifiersFromArray:(NSArray *)tracks;
		[Static]
		[Export ("identifiersFromArray:")]
		// [Verify (StronglyTypedNSArray), Verify (StronglyTypedNSArray)]
		string[] IdentifiersFromArray (NSObject[] tracks);

		// +(NSArray *)urisFromArray:(NSArray *)tracks;
		[Static]
		[Export ("urisFromArray:")]
		// [Verify (StronglyTypedNSArray), Verify (StronglyTypedNSArray)]
		NSObject[] UrisFromArray (NSObject[] tracks);

		// +(NSArray *)uriStringsFromArray:(NSArray *)tracks;
		[Static]
		[Export ("uriStringsFromArray:")]
		// [Verify (StronglyTypedNSArray), Verify (StronglyTypedNSArray)]
		NSObject[] UriStringsFromArray (NSObject[] tracks);
	}

	// @interface SPTPlaylistTrack : SPTTrack <SPTJSONObject>
	[BaseType (typeof(SPTTrack))]
	interface SPTPlaylistTrack : ISPTJSONObject
	{
		// @property (readonly, copy, nonatomic) NSDate * addedAt;
		[Export ("addedAt", ArgumentSemantic.Copy)]
		NSDate AddedAt { get; }

		// @property (readonly, nonatomic) SPTUser * addedBy;
		[Export ("addedBy")]
		SPTUser AddedBy { get; }
	}

	// @interface SPTSavedTrack : SPTTrack <SPTJSONObject>
	[BaseType (typeof(SPTTrack))]
	interface SPTSavedTrack : ISPTJSONObject
	{
		// @property (readonly, copy, nonatomic) NSDate * addedAt;
		[Export ("addedAt", ArgumentSemantic.Copy)]
		NSDate AddedAt { get; }
	}

	// @interface SPTUser : SPTJSONObjectBase
	[BaseType (typeof(SPTJSONObjectBase))]
	interface SPTUser
	{
		// @property (readonly, copy, nonatomic) NSString * displayName;
		[Export ("displayName")]
		string DisplayName { get; }

		// @property (readonly, copy, nonatomic) NSString * canonicalUserName;
		[Export ("canonicalUserName")]
		string CanonicalUserName { get; }

		// @property (readonly, copy, nonatomic) NSString * territory;
		[Export ("territory")]
		string Territory { get; }

		// @property (readonly, copy, nonatomic) NSString * emailAddress;
		[Export ("emailAddress")]
		string EmailAddress { get; }

		// @property (readonly, copy, nonatomic) NSURL * uri;
		[Export ("uri", ArgumentSemantic.Copy)]
		NSUrl Uri { get; }

		// @property (readonly, copy, nonatomic) NSURL * sharingURL;
		[Export ("sharingURL", ArgumentSemantic.Copy)]
		NSUrl SharingURL { get; }

		// @property (readonly, copy, nonatomic) NSArray * images;
		[Export ("images", ArgumentSemantic.Copy)]
		// [Verify (StronglyTypedNSArray)]
		SPTImage[] Images { get; }

		// @property (readonly, nonatomic) SPTImage * smallestImage;
		[Export ("smallestImage")]
		SPTImage SmallestImage { get; }

		// @property (readonly, nonatomic) SPTImage * largestImage;
		[Export ("largestImage")]
		SPTImage LargestImage { get; }

		// @property (readonly, nonatomic) SPTProduct product;
		[Export ("product")]
		SPTProduct Product { get; }

		// @property (readonly, nonatomic) long followerCount;
		[Export ("followerCount")]
		nint FollowerCount { get; }

		// +(NSURLRequest *)createRequestForCurrentUserWithAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForCurrentUserWithAccessToken:error:")]
		NSUrlRequest CreateRequestForCurrentUserWithAccessToken (string accessToken, out NSError error);

		// +(void)requestCurrentUserWithAccessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Static]
		[Export ("requestCurrentUserWithAccessToken:callback:")]
		void RequestCurrentUserWithAccessToken (string accessToken, SPTRequestCallback block);

		// +(void)requestUser:(NSString *)username withAccessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Static]
		[Export ("requestUser:withAccessToken:callback:")]
		void RequestUser (string username, string accessToken, SPTRequestCallback block);

		// +(instancetype)userFromData:(NSData *)data withResponse:(NSURLResponse *)response error:(NSError **)error;
		[Static]
		[Export ("userFromData:withResponse:error:")]
		SPTUser UserFromData (NSData data, NSUrlResponse response, out NSError error);

		// +(instancetype)userFromDecodedJSON:(id)decodedObject error:(NSError **)error;
		[Static]
		[Export ("userFromDecodedJSON:error:")]
		SPTUser UserFromDecodedJSON (NSObject decodedObject, out NSError error);
	}

	// @interface SPTFeaturedPlaylistList : SPTListPage
	[BaseType (typeof(SPTListPage))]
	interface SPTFeaturedPlaylistList
	{
		// @property (readonly, nonatomic) NSString * message;
		[Export ("message")]
		string Message { get; }

		// +(instancetype)featuredPlaylistListFromData:(NSData *)data withResponse:(NSURLResponse *)response error:(NSError **)error;
		[Static]
		[Export ("featuredPlaylistListFromData:withResponse:error:")]
		SPTFeaturedPlaylistList FeaturedPlaylistListFromData (NSData data, NSUrlResponse response, out NSError error);

		// +(instancetype)featuredPlaylistListFromDecodedJSON:(id)decodedObject error:(NSError **)error;
		[Static]
		[Export ("featuredPlaylistListFromDecodedJSON:error:")]
		SPTFeaturedPlaylistList FeaturedPlaylistListFromDecodedJSON (NSObject decodedObject, out NSError error);
	}

	// @interface SPTFollow : NSObject
	[BaseType (typeof(NSObject))]
	interface SPTFollow
	{
		// +(NSURLRequest *)createRequestForFollowingArtists:(NSArray *)artistUris withAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForFollowingArtists:withAccessToken:error:")]
		// [Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForFollowingArtists (NSObject[] artistUris, string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForUnfollowingArtists:(NSArray *)artistUris withAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForUnfollowingArtists:withAccessToken:error:")]
		// [Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForUnfollowingArtists (NSObject[] artistUris, string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForCheckingIfFollowingArtists:(NSArray *)artistUris withAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForCheckingIfFollowingArtists:withAccessToken:error:")]
		// [Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForCheckingIfFollowingArtists (NSObject[] artistUris, string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForFollowingUsers:(NSArray *)usernames withAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForFollowingUsers:withAccessToken:error:")]
		// [Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForFollowingUsers (NSObject[] usernames, string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForUnfollowingUsers:(NSArray *)usernames withAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForUnfollowingUsers:withAccessToken:error:")]
		// [Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForUnfollowingUsers (NSObject[] usernames, string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForCheckingIfFollowingUsers:(NSArray *)username withAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForCheckingIfFollowingUsers:withAccessToken:error:")]
		// [Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForCheckingIfFollowingUsers (NSObject[] username, string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForFollowingPlaylist:(NSURL *)playlistUri withAccessToken:(NSString *)accessToken secret:(BOOL)secret error:(NSError **)error;
		[Static]
		[Export ("createRequestForFollowingPlaylist:withAccessToken:secret:error:")]
		NSUrlRequest CreateRequestForFollowingPlaylist (NSUrl playlistUri, string accessToken, bool secret, out NSError error);

		// +(NSURLRequest *)createRequestForUnfollowingPlaylist:(NSURL *)playlistUri withAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForUnfollowingPlaylist:withAccessToken:error:")]
		NSUrlRequest CreateRequestForUnfollowingPlaylist (NSUrl playlistUri, string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForCheckingIfUsers:(NSArray *)usernames areFollowingPlaylist:(NSURL *)playlistUri withAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForCheckingIfUsers:areFollowingPlaylist:withAccessToken:error:")]
		// [Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForCheckingIfUsers (NSObject[] usernames, NSUrl playlistUri, string accessToken, out NSError error);

		// +(NSArray *)followingResultFromData:(NSData *)data withResponse:(NSURLResponse *)response error:(NSError **)error;
		[Static]
		[Export ("followingResultFromData:withResponse:error:")]
		// [Verify (StronglyTypedNSArray)]
		NSObject[] FollowingResultFromData (NSData data, NSUrlResponse response, out NSError error);
	}

	// @interface SPTBrowse : NSObject
	[BaseType (typeof(NSObject))]
	interface SPTBrowse
	{
		// +(NSURLRequest *)createRequestForFeaturedPlaylistsInCountry:(NSString *)country limit:(NSInteger)limit offset:(NSInteger)offset locale:(NSString *)locale timestamp:(NSDate *)timestamp accessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForFeaturedPlaylistsInCountry:limit:offset:locale:timestamp:accessToken:error:")]
		NSUrlRequest CreateRequestForFeaturedPlaylistsInCountry (string country, nint limit, nint offset, string locale, NSDate timestamp, string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForNewReleasesInCountry:(NSString *)country limit:(NSInteger)limit offset:(NSInteger)offset accessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForNewReleasesInCountry:limit:offset:accessToken:error:")]
		NSUrlRequest CreateRequestForNewReleasesInCountry (string country, nint limit, nint offset, string accessToken, out NSError error);

		// +(SPTListPage *)newReleasesFromData:(NSData *)data withResponse:(NSURLResponse *)response error:(NSError **)error;
		[Static]
		[Export ("newReleasesFromData:withResponse:error:")]
		SPTListPage NewReleasesFromData (NSData data, NSUrlResponse response, out NSError error);

		// +(void)requestFeaturedPlaylistsForCountry:(NSString *)country limit:(NSInteger)limit offset:(NSInteger)offset locale:(NSString *)locale timestamp:(NSDate *)timestamp accessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Static]
		[Export ("requestFeaturedPlaylistsForCountry:limit:offset:locale:timestamp:accessToken:callback:")]
		void RequestFeaturedPlaylistsForCountry (string country, nint limit, nint offset, string locale, NSDate timestamp, string accessToken, SPTRequestCallback block);

		// +(void)requestNewReleasesForCountry:(NSString *)country limit:(NSInteger)limit offset:(NSInteger)offset accessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Static]
		[Export ("requestNewReleasesForCountry:limit:offset:accessToken:callback:")]
		void RequestNewReleasesForCountry (string country, nint limit, nint offset, string accessToken, SPTRequestCallback block);
	}

	// @interface SPTYourMusic : NSObject
	[BaseType (typeof(NSObject))]
	interface SPTYourMusic
	{
		// +(NSURLRequest *)createRequestForCurrentUsersSavedTracksWithAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForCurrentUsersSavedTracksWithAccessToken:error:")]
		NSUrlRequest CreateRequestForCurrentUsersSavedTracksWithAccessToken (string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForSavingTracks:(NSArray *)tracks forUserWithAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForSavingTracks:forUserWithAccessToken:error:")]
		// [Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForSavingTracks (NSObject[] tracks, string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForCheckingIfSavedTracksContains:(NSArray *)tracks forUserWithAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForCheckingIfSavedTracksContains:forUserWithAccessToken:error:")]
		// [Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForCheckingIfSavedTracksContains (NSObject[] tracks, string accessToken, out NSError error);

		// +(NSURLRequest *)createRequestForRemovingTracksFromSaved:(NSArray *)tracks forUserWithAccessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForRemovingTracksFromSaved:forUserWithAccessToken:error:")]
		// [Verify (StronglyTypedNSArray)]
		NSUrlRequest CreateRequestForRemovingTracksFromSaved (NSObject[] tracks, string accessToken, out NSError error);

		// +(void)savedTracksForUserWithAccessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Static]
		[Export ("savedTracksForUserWithAccessToken:callback:")]
		void SavedTracksForUserWithAccessToken (string accessToken, SPTRequestCallback block);

		// +(void)saveTracks:(NSArray *)tracks forUserWithAccessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Static]
		[Export ("saveTracks:forUserWithAccessToken:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void SaveTracks (NSObject[] tracks, string accessToken, SPTRequestCallback block);

		// +(void)savedTracksContains:(NSArray *)tracks forUserWithAccessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Static]
		[Export ("savedTracksContains:forUserWithAccessToken:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void SavedTracksContains (NSObject[] tracks, string accessToken, SPTRequestCallback block);

		// +(void)removeTracksFromSaved:(NSArray *)tracks forUserWithAccessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Static]
		[Export ("removeTracksFromSaved:forUserWithAccessToken:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void RemoveTracksFromSaved (NSObject[] tracks, string accessToken, SPTRequestCallback block);
	}

	// @interface SPTSearch : NSObject
	[BaseType (typeof(NSObject))]
	interface SPTSearch
	{
		// +(void)performSearchWithQuery:(NSString *)searchQuery queryType:(SPTSearchQueryType)searchQueryType offset:(NSInteger)offset accessToken:(NSString *)accessToken market:(NSString *)market callback:(SPTRequestCallback)block;
		[Static]
		[Export ("performSearchWithQuery:queryType:offset:accessToken:market:callback:")]
		void PerformSearchWithQuery (string searchQuery, SPTSearchQueryType searchQueryType, nint offset, string accessToken, string market, SPTRequestCallback block);

		// +(NSURLRequest *)createRequestForSearchWithQuery:(NSString *)searchQuery queryType:(SPTSearchQueryType)searchQueryType offset:(NSInteger)offset accessToken:(NSString *)accessToken market:(NSString *)market error:(NSError **)error;
		[Static]
		[Export ("createRequestForSearchWithQuery:queryType:offset:accessToken:market:error:")]
		NSUrlRequest CreateRequestForSearchWithQuery (string searchQuery, SPTSearchQueryType searchQueryType, nint offset, string accessToken, string market, out NSError error);

		// +(void)performSearchWithQuery:(NSString *)searchQuery queryType:(SPTSearchQueryType)searchQueryType accessToken:(NSString *)accessToken market:(NSString *)market callback:(SPTRequestCallback)block;
		[Static]
		[Export ("performSearchWithQuery:queryType:accessToken:market:callback:")]
		void PerformSearchWithQuery (string searchQuery, SPTSearchQueryType searchQueryType, string accessToken, string market, SPTRequestCallback block);

		// +(NSURLRequest *)createRequestForSearchWithQuery:(NSString *)searchQuery queryType:(SPTSearchQueryType)searchQueryType accessToken:(NSString *)accessToken market:(NSString *)market error:(NSError **)error;
		[Static]
		[Export ("createRequestForSearchWithQuery:queryType:accessToken:market:error:")]
		NSUrlRequest CreateRequestForSearchWithQuery (string searchQuery, SPTSearchQueryType searchQueryType, string accessToken, string market, out NSError error);

		// +(void)performSearchWithQuery:(NSString *)searchQuery queryType:(SPTSearchQueryType)searchQueryType offset:(NSInteger)offset accessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Static]
		[Export ("performSearchWithQuery:queryType:offset:accessToken:callback:")]
		void PerformSearchWithQuery (string searchQuery, SPTSearchQueryType searchQueryType, nint offset, string accessToken, SPTRequestCallback block);

		// +(NSURLRequest *)createRequestForSearchWithQuery:(NSString *)searchQuery queryType:(SPTSearchQueryType)searchQueryType offset:(NSInteger)offset accessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForSearchWithQuery:queryType:offset:accessToken:error:")]
		NSUrlRequest CreateRequestForSearchWithQuery (string searchQuery, SPTSearchQueryType searchQueryType, nint offset, string accessToken, out NSError error);

		// +(void)performSearchWithQuery:(NSString *)searchQuery queryType:(SPTSearchQueryType)searchQueryType accessToken:(NSString *)accessToken callback:(SPTRequestCallback)block;
		[Static]
		[Export ("performSearchWithQuery:queryType:accessToken:callback:")]
		void PerformSearchWithQuery (string searchQuery, SPTSearchQueryType searchQueryType, string accessToken, SPTRequestCallback block);

		// +(NSURLRequest *)createRequestForSearchWithQuery:(NSString *)searchQuery queryType:(SPTSearchQueryType)searchQueryType accessToken:(NSString *)accessToken error:(NSError **)error;
		[Static]
		[Export ("createRequestForSearchWithQuery:queryType:accessToken:error:")]
		NSUrlRequest CreateRequestForSearchWithQuery (string searchQuery, SPTSearchQueryType searchQueryType, string accessToken, out NSError error);

		// +(SPTListPage *)searchResultsFromData:(NSData *)data withResponse:(NSURLResponse *)response queryType:(SPTSearchQueryType)searchQueryType error:(NSError **)error;
		[Static]
		[Export ("searchResultsFromData:withResponse:queryType:error:")]
		SPTListPage SearchResultsFromData (NSData data, NSUrlResponse response, SPTSearchQueryType searchQueryType, out NSError error);

		// +(SPTListPage *)searchResultsFromDecodedJSON:(id)decodedObject queryType:(SPTSearchQueryType)searchQueryType error:(NSError **)error;
		[Static]
		[Export ("searchResultsFromDecodedJSON:queryType:error:")]
		SPTListPage SearchResultsFromDecodedJSON (NSObject decodedObject, SPTSearchQueryType searchQueryType, out NSError error);
	}

	// @interface SPTCircularBuffer : NSObject
	[BaseType (typeof(NSObject))]
	interface SPTCircularBuffer
	{
		// -(id)initWithMaximumLength:(NSUInteger)size;
		[Export ("initWithMaximumLength:")]
		IntPtr Constructor (nuint size);

		// -(void)clear;
		[Export ("clear")]
		void Clear ();

		// -(NSUInteger)attemptAppendData:(const void *)data ofLength:(NSUInteger)dataLength;
//		[Export ("attemptAppendData:ofLength:")]
//		unsafe nuint AttemptAppendData (void* data, nuint dataLength);

		// -(NSUInteger)attemptAppendData:(const void *)data ofLength:(NSUInteger)dataLength chunkSize:(NSUInteger)chunkSize;
//		[Export ("attemptAppendData:ofLength:chunkSize:")]
//		unsafe nuint AttemptAppendData (void* data, nuint dataLength, nuint chunkSize);

		// -(NSUInteger)readDataOfLength:(NSUInteger)desiredLength intoAllocatedBuffer:(void **)outBuffer;
//		[Export ("readDataOfLength:intoAllocatedBuffer:")]
//		unsafe nuint ReadDataOfLength (nuint desiredLength, void** outBuffer);

		// @property (readonly) NSUInteger length;
		[Export ("length")]
		nuint Length { get; }

		// @property (readonly, nonatomic) NSUInteger maximumLength;
		[Export ("maximumLength")]
		nuint MaximumLength { get; }
	}

	// @protocol SPTCoreAudioControllerDelegate <NSObject>
	[Protocol, Model]
	[BaseType (typeof(NSObject))]
	interface SPTCoreAudioControllerDelegate
	{
		// @optional -(void)coreAudioController:(SPTCoreAudioController *)controller didOutputAudioOfDuration:(NSTimeInterval)audioDuration;
		[Export ("coreAudioController:didOutputAudioOfDuration:")]
		void DidOutputAudioOfDuration (SPTCoreAudioController controller, double audioDuration);
	}

	// @interface SPTCoreAudioController : NSObject
	[BaseType (typeof(NSObject))]
	interface SPTCoreAudioController
	{
		// -(void)clearAudioBuffers;
		[Export ("clearAudioBuffers")]
		void ClearAudioBuffers ();

		// -(NSInteger)attemptToDeliverAudioFrames:(const void *)audioFrames ofCount:(NSInteger)frameCount streamDescription:(AudioStreamBasicDescription)audioDescription;
		// [Export ("attemptToDeliverAudioFrames:ofCount:streamDescription:")]
		// unsafe nint AttemptToDeliverAudioFrames (void* audioFrames, nint frameCount, AudioStreamBasicDescription audioDescription);

		// -(uint32_t)bytesInAudioBuffer;
		[Export ("bytesInAudioBuffer")]
		// [Verify (MethodToProperty)]
		uint BytesInAudioBuffer { get; }

		// -(BOOL)connectOutputBus:(UInt32)sourceOutputBusNumber ofNode:(AUNode)sourceNode toInputBus:(UInt32)destinationInputBusNumber ofNode:(AUNode)destinationNode inGraph:(AUGraph)graph error:(NSError **)error;
		// [Export ("connectOutputBus:ofNode:toInputBus:ofNode:inGraph:error:")]
		// unsafe bool ConnectOutputBus (uint sourceOutputBusNumber, int sourceNode, uint destinationInputBusNumber, int destinationNode, AUGraph* graph, out NSError error);

		// -(void)disposeOfCustomNodesInGraph:(AUGraph)graph;
		// [Export ("disposeOfCustomNodesInGraph:")]
		// unsafe void DisposeOfCustomNodesInGraph (AUGraph* graph);

		// @property (readwrite, nonatomic) double volume;
		[Export ("volume")]
		double Volume { get; set; }

		// @property (readwrite, nonatomic) BOOL audioOutputEnabled;
		[Export ("audioOutputEnabled")]
		bool AudioOutputEnabled { get; set; }

		[Wrap ("WeakDelegate")]
		SPTCoreAudioControllerDelegate Delegate { get; set; }

		// @property (readwrite, nonatomic, weak) id<SPTCoreAudioControllerDelegate> delegate;
		[NullAllowed, Export ("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }

		// @property (readwrite, nonatomic) UIBackgroundTaskIdentifier backgroundPlaybackTask;
		[Export ("backgroundPlaybackTask")]
		nuint BackgroundPlaybackTask { get; set; }
	}

	partial interface Constants
	{
		// extern const NSUInteger SPTDiskCacheBlockSize;
		[Field ("SPTDiskCacheBlockSize", "__Internal")]
		nuint SPTDiskCacheBlockSize { get; }
	}

	// @protocol SPTCacheData <NSObject>
	[Protocol, Model]
	[BaseType (typeof(NSObject))]
	interface SPTCacheData
	{
		// @required @property (copy, nonatomic) NSURL * URI;
		[Abstract]
		[Export ("URI", ArgumentSemantic.Copy)]
		NSUrl URI { get; set; }

		// @required @property (copy, nonatomic) NSString * itemID;
		[Abstract]
		[Export ("itemID")]
		string ItemID { get; set; }

		// @required @property (nonatomic) NSUInteger offset;
		[Abstract]
		[Export ("offset")]
		nuint Offset { get; set; }

		// @required @property (copy, nonatomic) NSData * data;
		[Abstract]
		[Export ("data", ArgumentSemantic.Copy)]
		NSData Data { get; set; }

		// @required @property (nonatomic) NSUInteger totalSize;
		[Abstract]
		[Export ("totalSize")]
		nuint TotalSize { get; set; }
	}

	// @protocol SPTDiskCaching <NSObject>
	[Protocol, Model]
	[BaseType (typeof(NSObject))]
	interface SPTDiskCaching
	{
		// @required -(id<SPTCacheData>)readCacheDataWithURI:(NSURL *)URI itemID:(NSString *)itemID length:(NSUInteger)length offset:(NSUInteger)offset;
		[Abstract]
		[Export ("readCacheDataWithURI:itemID:length:offset:")]
		SPTCacheData ReadCacheDataWithURI (NSUrl URI, string itemID, nuint length, nuint offset);

		// @required -(BOOL)writeCacheData:(id<SPTCacheData>)cacheData;
		[Abstract]
		[Export ("writeCacheData:")]
		bool WriteCacheData (SPTCacheData cacheData);
	}

	interface ISPTDiskCaching : SPTDiskCaching
	{ }

	// @interface SPTDiskCache : NSObject <SPTDiskCaching>
	[BaseType (typeof(NSObject))]
	interface SPTDiskCache : ISPTDiskCaching
	{
		// -(instancetype)initWithCapacity:(NSUInteger)capacity;
		[Export ("initWithCapacity:")]
		IntPtr Constructor (nuint capacity);

		// -(BOOL)evict:(NSError **)error;
		[Export ("evict:")]
		bool Evict (out NSError error);

		// -(BOOL)clear:(NSError **)error;
		[Export ("clear:")]
		bool Clear (out NSError error);

		// -(NSUInteger)size;
		[Export ("size")]
		// [Verify (MethodToProperty)]
		nuint Size { get; }

		// @property (readonly, nonatomic) NSUInteger capacity;
		[Export ("capacity")]
		nuint Capacity { get; }
	}

	// @interface SPTPlayOptions : NSObject
	[BaseType (typeof(NSObject))]
	interface SPTPlayOptions
	{
		// @property (readwrite, nonatomic) int trackIndex;
		[Export ("trackIndex")]
		int TrackIndex { get; set; }

		// @property (readwrite, nonatomic) NSTimeInterval startTime;
		[Export ("startTime")]
		double StartTime { get; set; }
	}

	partial interface Constants
	{
		// extern NSString *const SPTAudioStreamingMetadataTrackName __attribute__((deprecated("")));
		[Field ("SPTAudioStreamingMetadataTrackName", "__Internal")]
		NSString SPTAudioStreamingMetadataTrackName { get; }

		// extern NSString *const SPTAudioStreamingMetadataTrackURI;
		[Field ("SPTAudioStreamingMetadataTrackURI", "__Internal")]
		NSString SPTAudioStreamingMetadataTrackURI { get; }

		// extern NSString *const SPTAudioStreamingMetadataArtistName __attribute__((deprecated("")));
		[Field ("SPTAudioStreamingMetadataArtistName", "__Internal")]
		NSString SPTAudioStreamingMetadataArtistName { get; }

		// extern NSString *const SPTAudioStreamingMetadataArtistURI __attribute__((deprecated("")));
		[Field ("SPTAudioStreamingMetadataArtistURI", "__Internal")]
		NSString SPTAudioStreamingMetadataArtistURI { get; }

		// extern NSString *const SPTAudioStreamingMetadataAlbumName __attribute__((deprecated("")));
		[Field ("SPTAudioStreamingMetadataAlbumName", "__Internal")]
		NSString SPTAudioStreamingMetadataAlbumName { get; }

		// extern NSString *const SPTAudioStreamingMetadataAlbumURI __attribute__((deprecated("")));
		[Field ("SPTAudioStreamingMetadataAlbumURI", "__Internal")]
		NSString SPTAudioStreamingMetadataAlbumURI { get; }

		// extern NSString *const SPTAudioStreamingMetadataTrackDuration __attribute__((deprecated("")));
		[Field ("SPTAudioStreamingMetadataTrackDuration", "__Internal")]
		NSString SPTAudioStreamingMetadataTrackDuration { get; }
	}

	// @interface SPTAudioStreamingController : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface SPTAudioStreamingController
	{
		// +(instancetype)sharedInstance;
		[Static]
		[Export ("sharedInstance")]
		SPTAudioStreamingController SharedInstance ();

		// -(BOOL)startWithClientId:(NSString *)clientId audioController:(SPTCoreAudioController *)audioController error:(NSError **)error;
		[Export ("startWithClientId:audioController:error:")]
		bool StartWithClientId (string clientId, SPTCoreAudioController audioController, out NSError error);

		// -(BOOL)startWithClientId:(NSString *)clientId error:(NSError **)error;
		[Export ("startWithClientId:error:")]
		bool StartWithClientId (string clientId, out NSError error);

		// -(BOOL)stopWithError:(NSError **)error;
		[Export ("stopWithError:")]
		bool StopWithError (out NSError error);

		// -(void)loginWithAccessToken:(NSString *)accessToken;
		[Export ("loginWithAccessToken:")]
		void LoginWithAccessToken (string accessToken);

		// -(void)logout;
		[Export ("logout")]
		void Logout ();

		// @property (readonly, assign, nonatomic) BOOL initialized;
		[Export ("initialized")]
		bool Initialized { get; }

		// @property (readonly, nonatomic) BOOL loggedIn;
		[Export ("loggedIn")]
		bool LoggedIn { get; }

		[Wrap ("WeakDelegate")]
		SPTAudioStreamingDelegate Delegate { get; set; }

		// @property (nonatomic, weak) id<SPTAudioStreamingDelegate> delegate;
		[NullAllowed, Export ("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }

		[Wrap ("WeakPlaybackDelegate")]
		SPTAudioStreamingPlaybackDelegate PlaybackDelegate { get; set; }

		// @property (nonatomic, weak) id<SPTAudioStreamingPlaybackDelegate> playbackDelegate;
		[NullAllowed, Export ("playbackDelegate", ArgumentSemantic.Weak)]
		NSObject WeakPlaybackDelegate { get; set; }

		// @property (nonatomic, strong) id<SPTDiskCaching> diskCache;
		[Export ("diskCache", ArgumentSemantic.Strong)]
		NSObject DiskCache { get; set; }

		// -(void)setVolume:(SPTVolume)volume callback:(SPTErrorableOperationCallback)block;
		[Export ("setVolume:callback:")]
		void SetVolume (double volume, SPTErrorableOperationCallback block);

		// -(void)setTargetBitrate:(SPTBitrate)bitrate callback:(SPTErrorableOperationCallback)block;
		[Export ("setTargetBitrate:callback:")]
		void SetTargetBitrate (SPTBitrate bitrate, SPTErrorableOperationCallback block);

		// -(void)seekToOffset:(NSTimeInterval)offset callback:(SPTErrorableOperationCallback)block;
		[Export ("seekToOffset:callback:")]
		void SeekToOffset (double offset, SPTErrorableOperationCallback block);

		// -(void)setIsPlaying:(BOOL)playing callback:(SPTErrorableOperationCallback)block;
		[Export ("setIsPlaying:callback:")]
		void SetIsPlaying (bool playing, SPTErrorableOperationCallback block);

		// -(void)playURI:(NSURL *)uri callback:(SPTErrorableOperationCallback)block __attribute__((deprecated("")));
		[Export ("playURI:callback:")]
		void PlayURI (NSUrl uri, SPTErrorableOperationCallback block);

		// -(void)playURI:(NSURL *)uri fromIndex:(int)index callback:(SPTErrorableOperationCallback)block __attribute__((deprecated("")));
		[Export ("playURI:fromIndex:callback:")]
		void PlayURI (NSUrl uri, int index, SPTErrorableOperationCallback block);

		// -(void)playURIs:(NSArray *)uris fromIndex:(int)index callback:(SPTErrorableOperationCallback)block;
		[Export ("playURIs:fromIndex:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void PlayURIs (NSUrl[] uris, int index, SPTErrorableOperationCallback block);

		// -(void)playURIs:(NSArray *)uris withOptions:(SPTPlayOptions *)options callback:(SPTErrorableOperationCallback)block;
		[Export ("playURIs:withOptions:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void PlayURIs (NSUrl[] uris, SPTPlayOptions options, SPTErrorableOperationCallback block);

		// -(void)setURIs:(NSArray *)uris callback:(SPTErrorableOperationCallback)block __attribute__((deprecated("")));
		[Export ("setURIs:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void SetURIs (NSUrl[] uris, SPTErrorableOperationCallback block);

		// -(void)replaceURIs:(NSArray *)uris withCurrentTrack:(int)index callback:(SPTErrorableOperationCallback)block;
		[Export ("replaceURIs:withCurrentTrack:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void ReplaceURIs (NSUrl[] uris, int index, SPTErrorableOperationCallback block);

		// -(void)playURIsFromIndex:(int)index callback:(SPTErrorableOperationCallback)block __attribute__((deprecated("")));
		[Export ("playURIsFromIndex:callback:")]
		void PlayURIsFromIndex (int index, SPTErrorableOperationCallback block);

		// -(void)playTrackProvider:(id<SPTTrackProvider>)provider callback:(SPTErrorableOperationCallback)block __attribute__((deprecated("")));
		[Export ("playTrackProvider:callback:")]
		void PlayTrackProvider (SPTTrackProvider provider, SPTErrorableOperationCallback block);

		// -(void)playTrackProvider:(id<SPTTrackProvider>)provider fromIndex:(int)index callback:(SPTErrorableOperationCallback)block __attribute__((deprecated("")));
		[Export ("playTrackProvider:fromIndex:callback:")]
		void PlayTrackProvider (SPTTrackProvider provider, int index, SPTErrorableOperationCallback block);

		// -(void)queueURI:(NSURL *)uri callback:(SPTErrorableOperationCallback)block __attribute__((deprecated("")));
		[Export ("queueURI:callback:")]
		void QueueURI (NSUrl uri, SPTErrorableOperationCallback block);

		// -(void)queueURI:(NSURL *)uri clearQueue:(BOOL)clear callback:(SPTErrorableOperationCallback)block __attribute__((deprecated("")));
		[Export ("queueURI:clearQueue:callback:")]
		void QueueURI (NSUrl uri, bool clear, SPTErrorableOperationCallback block);

		// -(void)queueURIs:(NSArray *)uris clearQueue:(BOOL)clear callback:(SPTErrorableOperationCallback)block;
		[Export ("queueURIs:clearQueue:callback:")]
		// [Verify (StronglyTypedNSArray)]
		void QueueURIs (NSUrl[] uris, bool clear, SPTErrorableOperationCallback block);

		// -(void)queueTrackProvider:(id<SPTTrackProvider>)provider clearQueue:(BOOL)clear callback:(SPTErrorableOperationCallback)block __attribute__((deprecated("")));
		[Export ("queueTrackProvider:clearQueue:callback:")]
		void QueueTrackProvider (SPTTrackProvider provider, bool clear, SPTErrorableOperationCallback block);

		// -(void)queuePlay:(SPTErrorableOperationCallback)block __attribute__((deprecated("")));
		[Export ("queuePlay:")]
		void QueuePlay (SPTErrorableOperationCallback block);

		// -(void)queueClear:(SPTErrorableOperationCallback)block __attribute__((deprecated("")));
		[Export ("queueClear:")]
		void QueueClear (SPTErrorableOperationCallback block);

		// -(void)stop:(SPTErrorableOperationCallback)block;
		[Export ("stop:")]
		void Stop (SPTErrorableOperationCallback block);

		// -(void)skipNext:(SPTErrorableOperationCallback)block;
		[Export ("skipNext:")]
		void SkipNext (SPTErrorableOperationCallback block);

		// -(void)skipPrevious:(SPTErrorableOperationCallback)block;
		[Export ("skipPrevious:")]
		void SkipPrevious (SPTErrorableOperationCallback block);

		// -(void)getRelativeTrackMetadata:(int)index callback:(void (^)(NSDictionary *))block __attribute__((deprecated("")));
		[Export ("getRelativeTrackMetadata:callback:")]
		void GetRelativeTrackMetadata (int index, Action<NSDictionary> block);

		// -(void)getAbsoluteTrackMetadata:(int)index callback:(void (^)(NSDictionary *))block __attribute__((deprecated("")));
		[Export ("getAbsoluteTrackMetadata:callback:")]
		void GetAbsoluteTrackMetadata (int index, Action<NSDictionary> block);

		// @property (readonly, copy, nonatomic) NSDictionary * currentTrackMetadata __attribute__((deprecated("")));
		[Export ("currentTrackMetadata", ArgumentSemantic.Copy)]
		NSDictionary CurrentTrackMetadata { get; }

		// @property (readonly, nonatomic) BOOL isPlaying;
		[Export ("isPlaying")]
		bool IsPlaying { get; }

		// @property (readonly, nonatomic) SPTVolume volume;
		[Export ("volume")]
		double Volume { get; }

		// @property (readwrite, nonatomic) BOOL shuffle;
		[Export ("shuffle")]
		bool Shuffle { get; set; }

		// @property (readwrite, nonatomic) BOOL repeat;
		[Export ("repeat")]
		bool Repeat { get; set; }

		// @property (readonly, nonatomic) NSTimeInterval currentPlaybackPosition;
		[Export ("currentPlaybackPosition")]
		double CurrentPlaybackPosition { get; }

		// @property (readonly, nonatomic) NSTimeInterval currentTrackDuration;
		[Export ("currentTrackDuration")]
		double CurrentTrackDuration { get; }

		// @property (readonly, nonatomic) NSURL * currentTrackURI;
		[Export ("currentTrackURI")]
		NSUrl CurrentTrackURI { get; }

		// @property (readonly, nonatomic) int currentTrackIndex;
		[Export ("currentTrackIndex")]
		int CurrentTrackIndex { get; }

		// @property (readonly, nonatomic) SPTBitrate targetBitrate;
		[Export ("targetBitrate")]
		SPTBitrate TargetBitrate { get; }

		// @property (readwrite, nonatomic) int trackListPosition __attribute__((deprecated("")));
		[Export ("trackListPosition")]
		int TrackListPosition { get; set; }

		// @property (readonly, nonatomic) int trackListSize;
		[Export ("trackListSize")]
		int TrackListSize { get; }

		// @property (readonly, nonatomic) int queueSize __attribute__((deprecated("")));
		[Export ("queueSize")]
		int QueueSize { get; }
	}

	// @protocol SPTAudioStreamingDelegate <NSObject>
	[Protocol, Model]
	[BaseType (typeof(NSObject))]
	interface SPTAudioStreamingDelegate
	{
		// @optional -(void)audioStreamingDidLogin:(SPTAudioStreamingController *)audioStreaming;
		[Export ("audioStreamingDidLogin:")]
		void AudioStreamingDidLogin (SPTAudioStreamingController audioStreaming);

		// @optional -(void)audioStreamingDidLogout:(SPTAudioStreamingController *)audioStreaming;
		[Export ("audioStreamingDidLogout:")]
		void AudioStreamingDidLogout (SPTAudioStreamingController audioStreaming);

		// @optional -(void)audioStreamingDidEncounterTemporaryConnectionError:(SPTAudioStreamingController *)audioStreaming;
		[Export ("audioStreamingDidEncounterTemporaryConnectionError:")]
		void AudioStreamingDidEncounterTemporaryConnectionError (SPTAudioStreamingController audioStreaming);

		// @optional -(void)audioStreaming:(SPTAudioStreamingController *)audioStreaming didEncounterError:(NSError *)error;
		[Export ("audioStreaming:didEncounterError:")]
		void AudioStreaming (SPTAudioStreamingController audioStreaming, NSError error);

		// @optional -(void)audioStreaming:(SPTAudioStreamingController *)audioStreaming didReceiveMessage:(NSString *)message;
		[Export ("audioStreaming:didReceiveMessage:")]
		void AudioStreaming (SPTAudioStreamingController audioStreaming, string message);

		// @optional -(void)audioStreamingDidDisconnect:(SPTAudioStreamingController *)audioStreaming;
		[Export ("audioStreamingDidDisconnect:")]
		void AudioStreamingDidDisconnect (SPTAudioStreamingController audioStreaming);

		// @optional -(void)audioStreamingDidReconnect:(SPTAudioStreamingController *)audioStreaming;
		[Export ("audioStreamingDidReconnect:")]
		void AudioStreamingDidReconnect (SPTAudioStreamingController audioStreaming);
	}

	// @protocol SPTAudioStreamingPlaybackDelegate <NSObject>
	[Protocol, Model]
	[BaseType (typeof(NSObject))]
	interface SPTAudioStreamingPlaybackDelegate
	{
		// @optional -(void)audioStreaming:(SPTAudioStreamingController *)audioStreaming didChangePlaybackStatus:(BOOL)isPlaying;
		[Export ("audioStreaming:didChangePlaybackStatus:")]
		void AudioStreaming (SPTAudioStreamingController audioStreaming, bool isPlaying);

		// @optional -(void)audioStreaming:(SPTAudioStreamingController *)audioStreaming didSeekToOffset:(NSTimeInterval)offset;
		[Export ("audioStreaming:didSeekToOffset:")]
		void AudioStreaming (SPTAudioStreamingController audioStreaming, double offset);

		// @optional -(void)audioStreaming:(SPTAudioStreamingController *)audioStreaming didChangeVolume:(SPTVolume)volume;
		[Export ("audioStreaming:didChangeVolume:")]
		void AudioStreamingDidChangeVolumne (SPTAudioStreamingController audioStreaming, double volume);

		// @optional -(void)audioStreaming:(SPTAudioStreamingController *)audioStreaming didChangeShuffleStatus:(BOOL)isShuffled;
		[Export ("audioStreaming:didChangeShuffleStatus:")]
		void AudioStreamingDidChangeShuffleStatus (SPTAudioStreamingController audioStreaming, bool isShuffled);

		// @optional -(void)audioStreaming:(SPTAudioStreamingController *)audioStreaming didChangeRepeatStatus:(BOOL)isRepeated;
		[Export ("audioStreaming:didChangeRepeatStatus:")]
		void AudioStreamingDidChangeRepeatStatus (SPTAudioStreamingController audioStreaming, bool isRepeated);

		// @optional -(void)audioStreaming:(SPTAudioStreamingController *)audioStreaming didChangeToTrack:(NSDictionary *)trackMetadata;
		[Export ("audioStreaming:didChangeToTrack:")]
		void AudioStreaming (SPTAudioStreamingController audioStreaming, NSDictionary trackMetadata);

		// @optional -(void)audioStreaming:(SPTAudioStreamingController *)audioStreaming didFailToPlayTrack:(NSURL *)trackUri;
		[Export ("audioStreaming:didFailToPlayTrack:")]
		void AudioStreamingDidFailToPlayTrack (SPTAudioStreamingController audioStreaming, NSUrl trackUri);

		// @optional -(void)audioStreaming:(SPTAudioStreamingController *)audioStreaming didStartPlayingTrack:(NSURL *)trackUri;
		[Export ("audioStreaming:didStartPlayingTrack:")]
		void AudioStreamingDidStartPlayingTrack (SPTAudioStreamingController audioStreaming, NSUrl trackUri);

		// @optional -(void)audioStreaming:(SPTAudioStreamingController *)audioStreaming didStopPlayingTrack:(NSURL *)trackUri;
		[Export ("audioStreaming:didStopPlayingTrack:")]
		void AudioStreamingDidStopPlayingTrack (SPTAudioStreamingController audioStreaming, NSUrl trackUri);

		// @optional -(void)audioStreamingDidSkipToNextTrack:(SPTAudioStreamingController *)audioStreaming;
		[Export ("audioStreamingDidSkipToNextTrack:")]
		void AudioStreamingDidSkipToNextTrack (SPTAudioStreamingController audioStreaming);

		// @optional -(void)audioStreamingDidSkipToPreviousTrack:(SPTAudioStreamingController *)audioStreaming;
		[Export ("audioStreamingDidSkipToPreviousTrack:")]
		void AudioStreamingDidSkipToPreviousTrack (SPTAudioStreamingController audioStreaming);

		// @optional -(void)audioStreamingDidBecomeActivePlaybackDevice:(SPTAudioStreamingController *)audioStreaming;
		[Export ("audioStreamingDidBecomeActivePlaybackDevice:")]
		void AudioStreamingDidBecomeActivePlaybackDevice (SPTAudioStreamingController audioStreaming);

		// @optional -(void)audioStreamingDidBecomeInactivePlaybackDevice:(SPTAudioStreamingController *)audioStreaming;
		[Export ("audioStreamingDidBecomeInactivePlaybackDevice:")]
		void AudioStreamingDidBecomeInactivePlaybackDevice (SPTAudioStreamingController audioStreaming);

		// @optional -(void)audioStreamingDidLosePermissionForPlayback:(SPTAudioStreamingController *)audioStreaming;
		[Export ("audioStreamingDidLosePermissionForPlayback:")]
		void AudioStreamingDidLosePermissionForPlayback (SPTAudioStreamingController audioStreaming);

		// @optional -(void)audioStreamingDidPopQueue:(SPTAudioStreamingController *)audioStreaming;
		[Export ("audioStreamingDidPopQueue:")]
		void AudioStreamingDidPopQueue (SPTAudioStreamingController audioStreaming);
	}

	partial interface Constants
	{
		// extern const NSInteger SPTErrorCodeNoError;
		[Field ("SPTErrorCodeNoError", "__Internal")]
		nint SPTErrorCodeNoError { get; }

		// extern const NSInteger SPTErrorCodeFailed;
		[Field ("SPTErrorCodeFailed", "__Internal")]
		nint SPTErrorCodeFailed { get; }

		// extern const NSInteger SPTErrorCodeInitFailed;
		[Field ("SPTErrorCodeInitFailed", "__Internal")]
		nint SPTErrorCodeInitFailed { get; }

		// extern const NSInteger SPTErrorCodeWrongAPIVersion;
		[Field ("SPTErrorCodeWrongAPIVersion", "__Internal")]
		nint SPTErrorCodeWrongAPIVersion { get; }

		// extern const NSInteger SPTErrorCodeNullArgument;
		[Field ("SPTErrorCodeNullArgument", "__Internal")]
		nint SPTErrorCodeNullArgument { get; }

		// extern const NSInteger SPTErrorCodeInvalidArgument;
		[Field ("SPTErrorCodeInvalidArgument", "__Internal")]
		nint SPTErrorCodeInvalidArgument { get; }

		// extern const NSInteger SPTErrorCodeUninitialized;
		[Field ("SPTErrorCodeUninitialized", "__Internal")]
		nint SPTErrorCodeUninitialized { get; }

		// extern const NSInteger SPTErrorCodeAlreadyInitialized;
		[Field ("SPTErrorCodeAlreadyInitialized", "__Internal")]
		nint SPTErrorCodeAlreadyInitialized { get; }

		// extern const NSInteger SPTErrorCodeBadCredentials;
		[Field ("SPTErrorCodeBadCredentials", "__Internal")]
		nint SPTErrorCodeBadCredentials { get; }

		// extern const NSInteger SPTErrorCodeNeedsPremium;
		[Field ("SPTErrorCodeNeedsPremium", "__Internal")]
		nint SPTErrorCodeNeedsPremium { get; }

		// extern const NSInteger SPTErrorCodeTravelRestriction;
		[Field ("SPTErrorCodeTravelRestriction", "__Internal")]
		nint SPTErrorCodeTravelRestriction { get; }

		// extern const NSInteger SPTErrorCodeApplicationBanned;
		[Field ("SPTErrorCodeApplicationBanned", "__Internal")]
		nint SPTErrorCodeApplicationBanned { get; }

		// extern const NSInteger SPTErrorCodeGeneralLoginError;
		[Field ("SPTErrorCodeGeneralLoginError", "__Internal")]
		nint SPTErrorCodeGeneralLoginError { get; }

		// extern const NSInteger SPTErrorCodeUnsupported;
		[Field ("SPTErrorCodeUnsupported", "__Internal")]
		nint SPTErrorCodeUnsupported { get; }

		// extern const NSInteger SPTErrorCodeNotActiveDevice;
		[Field ("SPTErrorCodeNotActiveDevice", "__Internal")]
		nint SPTErrorCodeNotActiveDevice { get; }

		// extern const NSInteger SPTErrorCodeGeneralPlaybackError;
		[Field ("SPTErrorCodeGeneralPlaybackError", "__Internal")]
		nint SPTErrorCodeGeneralPlaybackError { get; }

		// extern const NSInteger SPTErrorCodePlaybackRateLimited;
		[Field ("SPTErrorCodePlaybackRateLimited", "__Internal")]
		nint SPTErrorCodePlaybackRateLimited { get; }

		// extern const NSInteger SPTErrorCodeTrackUnavailable;
		// [Field ("SPTErrorCodeTrackUnavailable", "__Internal")]
		// nint SPTErrorCodeTrackUnavailable { get; }
	}
}
