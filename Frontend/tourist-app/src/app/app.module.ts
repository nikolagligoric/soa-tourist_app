import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LoginComponent } from './login/login.component';
import { HomeComponent } from './home/home.component';
import { RegisterComponent } from './register/register.component';
import { BlockUserComponent } from './block-user/block-user.component';
import { ProfileComponent } from './profile/profile.component';
import { NavbarComponent } from './navbar/navbar.component';
import { UserProfileComponent } from './user-profile/user-profile.component';
import { ToursComponent } from './tours/tours.component';
import { CartComponent } from './cart/cart.component';
import { PurchasesComponent } from './purchases/purchases.component';
import { TourDetailsComponent } from './tour-details/tour-details.component';
import { ActiveTourComponent } from './active-tour/active-tour.component';
import { BlogComponent } from './blog/blog.component';
import { CurrentLocationComponent } from './current-location/current-location.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    HomeComponent,
    RegisterComponent,
    BlockUserComponent,
    ProfileComponent,
    NavbarComponent,
    UserProfileComponent,
    ToursComponent,
    CartComponent,
    PurchasesComponent,
    TourDetailsComponent,
    ActiveTourComponent
    BlogComponent,
    CurrentLocationComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
